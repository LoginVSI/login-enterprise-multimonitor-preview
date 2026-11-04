# KW25 source mapping

Generated adaptation, not runtime proven. Filenames, TARGETs, primary classes, frameworks and all original timer calls are retained. All twelve files use desktop-style ScriptBase with synchronous Execute, not Playwright or Selenium. The manifest records source and output SHA-256 plus ordered timer calls. Source files were read only; no Close source was read or packaged.

| File | Primary class | TARGET | Lifecycle / delta | Original timer calls |
| --- | --- | --- | --- | --- |
| KW25 Edge Run.cs | `Edge_Run` | `msedge` | Run; adapted | No explicit timers |
| KW25 Edge Start.cs | `Edge_Start` | `msedge ` | Start; adapted | StartTimer("Browser_Start");; StopTimer("Browser_Start"); |
| KW25 Excel Run.cs | `Excel_Run` | `excel.exe /s` | Run; adapted | StartTimer("Open_Window");; StopTimer("Open_Window");; StartTimer("Open_Excel_Document");; StopTimer("Open_Excel_Document");; StartTimer("Saving_file");; StopTimer("Saving_file"); |
| KW25 Excel Start.cs | `Excel_Start` | `excel.exe %temp%\LoginEnterprise\loginvsi.xlsx` | Start; adapted | No explicit timers |
| KW25 Outlook Run.cs | `Run_Outlook` | `outlook.exe /importprf %TEMP%\LoginPI\Outlook.prf` | Run; adapted | StartTimer("Open_Existing_Email");; StopTimer("Open_Existing_Email");; StartTimer("New_Email_Open");; StopTimer("New_Email_Open");; StartTimer("Insert_Picture_Dialog");; StopTimer("Insert_Picture_Dialog");; StartTimer("Add_Attachment_Dialog");; StopTimer("Add_Attachment_Dialog"); |
| KW25 Outlook Start.cs | `Outlook_Start` | `outlook.exe /importprf %TEMP%\LoginPI\Outlook.prf` | Start; adapted | No explicit timers |
| KW25 PowerPoint Run.cs | `PowerPoint_Run` | `powerpnt.exe /n` | Run; adapted | StartTimer("Open_Window");; StopTimer("Open_Window");; StartTimer("Open_Powerpoint_Document");; StopTimer("Open_Powerpoint_Document");; StartTimer("Insert_Picture_Dialog");; StopTimer("Insert_Picture_Dialog");; StartTimer("Save_As_Dialog");; StopTimer("Save_As_Dialog");; StartTimer("Saving_file");; StopTimer("Saving_file"); |
| KW25 PowerPoint Start.cs | `PowerPoint_Start` | `powerpnt.exe %temp%\LoginEnterprise\loginvsi.pptx` | Start; adapted | No explicit timers |
| KW25 Prep 2019 Office.cs | `Office_2019_Prepare` | `winword.exe /t` | Prepare; unchanged | No explicit timers |
| KW25 Prep 365 Microsoft Office.cs | `M365_Prepare` | `winword.exe /t` | Prepare; unchanged | No explicit timers |
| KW25 Word Run.cs | `Word_Run` | `winword.exe /t` | Run; adapted | StartTimer("Open_Window");; StopTimer("Open_Window");; StartTimer("Open_Word_Document");; StopTimer("Open_Word_Document");; StartTimer("Insert_Picture_Dialog");; StopTimer("Insert_Picture_Dialog");; StartTimer("Save_As_Dialog");; StopTimer("Save_As_Dialog");; StartTimer("Saving_file");; StopTimer("Saving_file"); |
| KW25 Word Start.cs | `Word_Start` | `winword.exe %temp%\LoginEnterprise\loginvsi.docx` | Start; adapted | No explicit timers |

Prepare: both alternatives are byte-identical. Their Word setup, registry/profile changes, optional cleanup and first-run handling remain intact. Neither stages the DLL nor touches allocation state.

Starts: all retain original downloads, launch commands, readiness waits, dialog handling, maximize/focus and content. A preflight rejects existing matching base windows before launch. After the last startup maximize, a unique process/class/title match must equal the source-selected HWND. Placement then calls DLL PlaceNext once before the original focus. Reflection loading occurs before startup actions. Each successful result writes its monitor count and index to KW25-<Application>.txt in the Preview directory. No HWND is persisted. Failures abort, with no automatic retry allocation.

Outlook retains Classic outlook.exe /importprf, PRF/PST downloads and TEMP rewriting, Inbox/rctrl_renwnd32/OUTLOOK selection, activation/sign-in handling. New Outlook is not compatible. Edge retains guest/no-session-restore, ten tabs, every source URL and PDF, original initialization wait and Browser_Start boundaries. Zero pre-existing Edge base windows and final unique HWND equality avoid relying on the ShellExecute launcher PID. Concurrent manual browser launches are outside the isolated test-session prerequisite.

Excel uses loginvsi/XLMAIN/EXCEL; Word uses loginvsi/OpusApp/WINWORD; PowerPoint uses loginvsi/PPTFrameClass/POWERPNT. First-run dialogs are never allocated. Placement follows their original startup readiness rather than introducing another fixed wait.

All five Run scripts need maintenance. Edge repeatedly maximizes; Outlook maximizes its Inbox including reminder recovery. Excel, Word and PowerPoint actually launch blank windows, close loginvsi/edited windows, reopen source files, and Save As on each iteration. Their source behavior replaces documents rather than simply reusing the Start HWND. The original launches, cleanup, menus, charts, document text, image insertion, slideshow and saves are preserved. PlaceOnMonitor follows maximize on durable document/Inbox/browser variables only. No placement is added to dialogs, compose/read windows, chart surfaces or slideshows. The PowerPoint slideshow retains Office's own display choice and needs environment verification.

The Run method loads its application's saved count and index before original actions. It never reads the global last-used index and never allocates. Fixed 1, 2, 3 and 4 monitor topologies use the actual returned index, not a modulo assumption or hard-coded application number. Run once is off and Leave application running is on for every Run workload.

Public-safety change: Outlook Run's two historical example recipient domains are replaced with reserved example.invalid addresses, as required by repository public-safety policy. Recipient content differs; composition structure and all other content remain. Original sources are untouched. This is the only content substitution. No measurement calls or boundaries were moved. Added placement and loading take wall-clock time outside explicit timers; EUX/cadence impact requires measurement in a real scenario.

Source limitations retained: English UI/title patterns (including the source Edge zero-width character), source Office dialog loops, broad source cleanup title masks, source continueOnError choices and screen-coordinate clicks. Use isolated disposable test profiles without unrelated application windows. Source first-run loops are not made bounded by this adaptation. No cleanup logic was modernized.

Final audit: Run now rejects missing/corrupt shared state or assignment-count mismatch before placement, rather than allowing DLL repair to hide state loss. Before every placement, a unique expected process/class/title must match the source-selected current HWND; null results are guarded explicitly. No original actions or timer boundaries changed. Monitor-count topology changes discovered inside the DLL remain a stop/restart condition. Same-count topology changes and concurrent manual launches are unsupported. The assignment format changed from one index line to count/index lines; previous records must be recreated through a fresh Start sequence.

The complete KW25 package retains Edge. An Office-plus-existing-KW22-Edge configuration is documented separately, with no assumed Edge implementation or runtime proof. Customer identities are deliberately excluded from public files.
