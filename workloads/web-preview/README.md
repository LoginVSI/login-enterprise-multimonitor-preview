# Web workload examples

These unsupported Preview examples show the two runtime-validated browser adaptation patterns. Browser automation still starts, controls, and stops the browser. Workload code identifies the browser's native top-level Windows window and passes only its HWND to the same browser-agnostic placement DLL used by desktop workloads.

- [`10-Place-Playwright-Chrome.cs`](10-Place-Playwright-Chrome.cs) is for a current Playwright Web Recorder workload based on `WebScriptBase`. It snapshots existing Chrome top-level HWNDs, awaits `StartBrowser()`, accepts exactly one newly observed native Chrome window, places it, and continues with Playwright browser activity before awaiting `StopBrowser()`.
- [`20-Place-Legacy-Selenium-Edge.cs`](20-Place-Legacy-Selenium-Edge.cs) demonstrates the backward-compatible Selenium path based on `ScriptBase`. It uses Login Enterprise `FindWindows()`/`IWindow`, places the newly attributable Edge window, focuses it for foreground interaction, continues with a Selenium navigation, and calls `StopBrowser()`.

Use the example matching the source workload. Do not convert frameworks or combine Playwright, Selenium, and WinApp automation in one workload. Keep the source browser magic comments and recorded interactions. Current Login Enterprise documentation lists Chrome and Microsoft Edge for Playwright Web Recorder workloads; this repository does not claim broader browser or version compatibility.

Both examples fail instead of moving an arbitrary browser window when ownership is absent or ambiguous. They do not terminate browser processes. For legacy Selenium, a compatible browser and WebDriver remain normal environment prerequisites and are not managed by the Multi-Monitor Preview.

Run [`../dll-backed/00-Prepare-MultiMonitor.cs`](../dll-backed/00-Prepare-MultiMonitor.cs) before either example. Replace the public sample URL and continuation action with the original workload's recorded content when adapting a real workload, then validate the result in Script Editor and in the intended Login Enterprise scenario.
