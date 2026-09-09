using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.PACS;

/// <summary>
/// Orthanc PACS provider via DICOMweb REST API.
/// Configure ORTHANC_BASE_URL, ORTHANC_USERNAME, ORTHANC_PASSWORD in environment.
/// Orthanc docs: https://orthanc-server.com/static.php?page=documentation
/// </summary>
public class OrthancPacsProvider : IPacsProvider
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<OrthancPacsProvider> _logger;

    private string BaseUrl => _config["ORTHANC_BASE_URL"] ?? "http://localhost:8042";
    private string ViewerUrl => _config["OHIF_VIEWER_URL"] ?? string.Empty;

    public OrthancPacsProvider(HttpClient http, IConfiguration config, ILogger<OrthancPacsProvider> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;

        var user = _config["ORTHANC_USERNAME"];
        var pass = _config["ORTHANC_PASSWORD"];
        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{pass}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
    }

    public async Task<DicomUploadResult> UploadDicomFileAsync(Stream dicomStream, string filename, CancellationToken ct = default)
    {
        try
        {
            using var content = new StreamContent(dicomStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/dicom");

            var response = await _http.PostAsync($"{BaseUrl}/instances", content, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("PACS upload failed: {Error}", err);
                return new DicomUploadResult(false, null, null, null, err);
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            return new DicomUploadResult(
                true,
                root.GetProperty("ParentStudy").GetString(),
                root.GetProperty("ParentSeries").GetString(),
                root.GetProperty("ID").GetString(),
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PACS upload exception for file {Filename}", filename);
            return new DicomUploadResult(false, null, null, null, ex.Message);
        }
    }

    public async Task<List<DicomStudyMetadata>> QueryStudiesAsync(string patientId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync(
                $"{BaseUrl}/dicom-web/studies?PatientID={Uri.EscapeDataString(patientId)}", ct);

            if (!response.IsSuccessStatusCode)
                return new List<DicomStudyMetadata>();

            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);

            var results = new List<DicomStudyMetadata>();
            foreach (var study in doc.RootElement.EnumerateArray())
            {
                results.Add(new DicomStudyMetadata(
                    StudyInstanceUid: GetDicomTag(study, "0020000D"),
                    AccessionNumber: GetDicomTag(study, "00080050"),
                    StudyDescription: GetDicomTag(study, "00081030"),
                    StudyDate: ParseDicomDate(GetDicomTag(study, "00080020")),
                    Modality: GetDicomTag(study, "00080060") ?? "UNKNOWN",
                    SeriesCount: 0,
                    InstanceCount: 0
                ));
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PACS query failed for patient {PatientId}", patientId);
            return new List<DicomStudyMetadata>();
        }
    }

    public string GetWadoRsUrl(string studyUid, string seriesUid, string instanceUid, int frame = 1)
        => $"{BaseUrl}/dicom-web/studies/{studyUid}/series/{seriesUid}/instances/{instanceUid}/frames/{frame}";

    public string? GetViewerUrl(string studyUid)
        => string.IsNullOrEmpty(ViewerUrl)
            ? null
            : $"{ViewerUrl}/viewer?StudyInstanceUIDs={studyUid}";

    public async Task<bool> DeleteStudyAsync(string studyUid, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/dicom-web/studies/{studyUid}", ct);
        return response.IsSuccessStatusCode;
    }

    private static string? GetDicomTag(JsonElement element, string tag)
    {
        if (element.TryGetProperty(tag, out var tagEl)
            && tagEl.TryGetProperty("Value", out var values)
            && values.ValueKind == JsonValueKind.Array
            && values.GetArrayLength() > 0)
        {
            return values[0].ValueKind == JsonValueKind.String
                ? values[0].GetString()
                : values[0].ToString();
        }
        return null;
    }

    private static DateTime? ParseDicomDate(string? dicomDate)
    {
        if (string.IsNullOrEmpty(dicomDate) || dicomDate.Length < 8) return null;
        if (DateTime.TryParseExact(dicomDate, "yyyyMMdd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dt))
            return dt;
        return null;
    }
}
