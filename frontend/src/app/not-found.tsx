import Link from 'next/link';

export default function NotFound() {
  return (
    <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center p-4 text-center">
      <h1 className="text-4xl font-bold text-white mb-2">404 - Page Not Found</h1>
      <p className="text-slate-400 mb-6 text-sm">The clinical page or record you are looking for does not exist.</p>
      <Link
        href="/dashboard"
        className="px-4 py-2 bg-sky-600 hover:bg-sky-500 text-white text-xs font-semibold rounded-lg transition-colors"
      >
        Return to Dashboard
      </Link>
    </div>
  );
}
