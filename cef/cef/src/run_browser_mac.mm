#include "include/run_browser.h"

#import <Cocoa/Cocoa.h>

#include <include/cef_application_mac.h>
#include <include/wrapper/cef_helpers.h>
#include <include/wrapper/cef_library_loader.h>

#include <memory>

#include "internal/core.h"
#include "internal/client_manager.h"
#include "internal/browser_app.h"

// namespace synthesis {
// namespace {

@interface SharedAppDelegate : NSObject <NSApplicationDelegate>
- (void)createApplication:(id)object;
- (void)tryToTerminateApplication:(NSApplication*)app;
@end

@interface sharedApplication : NSApplication <CefAppProtocol> {
    @private
    BOOL handling_send_event;
}
@end

@implementation sharedApplication
- (BOOL)isHandlingSendEvent {
    return handling_send_event;
}

- (void)setHandlingSendEvent:(BOOL)flag {
    handling_send_event = flag;
}

- (void)sendEvent:(NSEvent*)event {
    CefScopedSendingEvent sending_event_scoper;
    [super sendEvent:event];
}

- (void)terminate:(id)sender {
    SharedAppDelegate* delegate = static_cast<SharedAppDelegate*>([NSApp delegate]);
    [delegate tryToTerminateApplication:self];
}
@end

@implementation SharedAppDelegate
- (void)createApplication:(id)object {
    [NSApplication sharedApplication];
    [[NSBundle mainBundle] loadNibNamed:@"MainMenu" owner:NSApp topLevelObjects:nil];

    [[NSApplication sharedApplication] setDelegate:self];
}

- (void)tryToTerminateApplication:(NSApplication*)app {
    // auto manager = synthesis::ClientManager::GetInstance();

    // if (manager && !manager->IsClosing()) {
    //     manager->CloseAllBrowsers(false);
    // }
}

- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication*)sender {
    return NSTerminateNow;
}
@end

// } // namespace
// } // namespace synthesis

#include <iostream>
using namespace std;

SYNTHESIS_EXPORT void StartBrowserClient() {

    cout << "StartBrowserClient()" << endl;

    CefScopedLibraryLoader library_loader;

    cout << "CefScopedLibraryLoader" << endl;

    if (!library_loader.LoadInMain()) {
        return;
    }

    NSAutoreleasePool* autopool = [[NSAutoreleasePool alloc] init];

    cout << "NSAutoreleasePool" << endl;

    int argc = 0;
    char* argv[] = {};
    CefMainArgs main_args(argc, argv);

    cout << "CefMainArgs" << endl;

    CefRefPtr<CefApp> app = synthesis::CreateBrowserProcessApp();

    cout << "CefRefPtr<CefApp>" << endl;

    [sharedApplication sharedApplication];

    cout << "[sharedApplication sharedApplication]" << endl;

    // synthesis::ClientManager manager;

    cout << "synthesis::ClientManager" << endl;

    CefSettings settings;

    cout << "CefSettings" << endl;

    CefInitialize(main_args, settings, app, nullptr);

    cout << "CefInitialize" << endl;

    NSObject* delegate = [[SharedAppDelegate alloc] init];
    [delegate performSelectorOnMainThread:@selector(createApplication:) withObject:nil waitUntilDone:NO];

    cout << "[delegate performSelectorOnMainThread:@selector(createApplication:) withObject:nil waitUntilDone:NO]" << endl;

    CefRunMessageLoop();

    cout << "CefRunMessageLoop()" << endl;

    CefShutdown();

    cout << "CefShutdown()" << endl;

    [delegate release];
    [autopool release]; 

    cout << "DONE!" << endl;
}
