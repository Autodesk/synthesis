#include "shared_main.h"

#import <Cocoa/Cocoa.h>

#include <include/cef_application_mac.h>
#include <include/wrapper/cef_helpers.h>
#include <include/wrapper/cef_library_loader.h>

#include "app_factory.h"
#include "client_manager.h"
#include "core.h"
#include "debug.h"
#include "html_resource_handler.h"

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
    auto manager = synthesis::shared::ClientManager::GetInstance();

    if (manager && !manager->IsClosing()) {
        manager->CloseAllBrowsers(false);
    }
}

- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication*)sender {
    return NSTerminateNow;
}
@end

namespace synthesis {
namespace shared {

int main(int argc, char* argv[]) {
    CefScopedLibraryLoader library_loader;

    if (!library_loader.LoadInMain()) {
        return 1;
    }

    NSAutoreleasePool* autopool = [[NSAutoreleasePool alloc] init];
    CefMainArgs main_args(argc, argv);
    CefRefPtr<CefApp> app = synthesis::shared::CreateBrowserProcessApp();

    [sharedApplication sharedApplication];
    synthesis::shared::ClientManager manager;
    CefSettings settings;

    CefInitialize(main_args, settings, app, nullptr);

    // CefRefPtr<HTMLSchemeHandlerFactory> resourceHandlerFactory = new HTMLSchemeHandlerFactory();
    // CefRegisterSchemeHandlerFactory(SYNTHESIS_HTML_SCHEME, "", resourceHandlerFactory);
    // std::string url = std::string(SYNTHESIS_HTML_SCHEME) + "index.html";

    NSObject* delegate = [[SharedAppDelegate alloc] init];
    [delegate performSelectorOnMainThread:@selector(createApplication:) withObject:nil waitUntilDone:NO];

    SYNTHESIS_DEBUG_LOG("Starting main loop...");

    CefRunMessageLoop();
    CefShutdown();

    [delegate release];
    [autopool release]; 

    SYNTHESIS_DEBUG_LOG("Main loop ended.");

    return 0;
}

} // namespace shared
} // namespace synthesis
