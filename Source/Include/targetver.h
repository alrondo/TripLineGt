
#ifndef TARGETVER_H_
#define TARGETVER_H_

// Including SDKDDKVer.h defines the highest available Windows platform.
// If you wish to build your application for a previous Windows platform, include WinSDKVer.h and
// set the _WIN32_WINNT macro to the platform you wish to support before including SDKDDKVer.h.

// DP-1000 NOTES
// Minimal Windows version supported is Windows 7.
// All source file including a .h from the Windows API MUST include targetver.h BEFORE that file.

#if defined(_WIN32) || defined(_WIN64)
#define _WIN32_WINNT NTDDI_WIN7

#endif

#endif
