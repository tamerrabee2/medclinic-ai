# DICOM / PACS Integration Setup

## Overview

MedClinic AI integrates with any DICOMweb-compliant PACS server.
Default development provider: **MockPacsProvider** (zero external dependencies).
Production provider: **OrthancPacsProvider** (Orthanc open-source PACS).

## Environment Variables

```env
# Orthanc PACS
ORTHANC_BASE_URL=http://localhost:8042
ORTHANC_USERNAME=orthanc
ORTHANC_PASSWORD=orthanc

# OHIF Viewer (optional)
OHIF_VIEWER_URL=http://localhost:3000

# Switch provider: Mock | Orthanc
PACS_PROVIDER=Mock
```

## Running Orthanc Locally

```yaml
# docker-compose.yml (add to existing services)
orthanc:
  image: orthancteam/orthanc:latest
  ports:
    - "8042:8042"
    - "4242:4242"
  environment:
    ORTHANC__DICOM_WEB__ENABLE: "true"
    ORTHANC__DICOM_WEB__ROOT_URL: "/dicom-web"
  volumes:
    - orthanc_data:/var/lib/orthanc/db
```

## Running OHIF Viewer

```bash
docker run -p 3000:80 ohif/app:latest
```

Configure OHIF to point to Orthanc DICOMweb URL.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET    | `/api/v1/patients/{id}/dicom` | List DICOM studies |
| POST   | `/api/v1/patients/{id}/dicom/upload` | Upload .dcm file to PACS |
| POST   | `/api/v1/patients/{id}/dicom/{studyId}/analyze` | Run AI analysis |
| POST   | `/api/v1/patients/{id}/dicom/{studyId}/approve-ai` | Doctor approves AI findings |

## DICOM Modalities Supported

| Code | Description |
|------|-------------|
| CT   | Computed Tomography |
| MR   | Magnetic Resonance |
| CR   | Computed Radiography |
| DX   | Digital X-Ray |
| US   | Ultrasound |
| PET  | Positron Emission Tomography |
| NM   | Nuclear Medicine |
| MG   | Mammography |

## AI Safety Policy

> AI imaging analysis output **always requires physician/radiologist review**
> before being included in any medical record or clinical decision.

```
AI Findings Generated
        ↓
 Doctor Reviews in UI
        ↓
 Approve / Reject
        ↓
  Saved to Record
```
