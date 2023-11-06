#include "shared_main.h"

#import <Cocoa/Cocoa.h>

#include <include/cef_application_mac.h>
#include <include/cef_browser.h>
#include <include/wrapper/cef_helpers.h>
#include <include/wrapper/cef_library_loader.h>

#include <numeric>

#include "app_factory.h"
#include "client_manager.h"
#include "core.h"
#include "debug.h"
#include "offscreen_cef_client.h"

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
    // CefRefPtr<CefApp> app = synthesis::shared::CreateRendererProcessApp();
    CefRefPtr<CefApp> app = nullptr;

    [sharedApplication sharedApplication];
    synthesis::shared::ClientManager manager;
    CefSettings settings;

    CefInitialize(main_args, settings, app, nullptr);

    // TODO: Remove
    // CefRefPtr<HTMLSchemeHandlerFactory> resourceHandlerFactory = new HTMLSchemeHandlerFactory();
    // CefRegisterSchemeHandlerFactory(SYNTHESIS_HTML_SCHEME, "", resourceHandlerFactory);
    // std::string url = std::string(SYNTHESIS_HTML_SCHEME) + "index.html";

    NSObject* delegate = [[SharedAppDelegate alloc] init];
    [delegate performSelectorOnMainThread:@selector(createApplication:) withObject:nil waitUntilDone:NO];

    SYNTHESIS_DEBUG_LOG("Starting main loop...");

    CefRefPtr<OffscreenCefClient> client(new OffscreenCefClient(500, 400));
    CefBrowserSettings browser_settings;
    CefWindowInfo window_info;
    window_info.SetAsWindowless(nullptr);

    CefBrowserHost::CreateBrowser(window_info, client, "http://www.google.com", browser_settings, nullptr, nullptr);

    SYNTHESIS_DEBUG_LOG("Browser texture bytes size: ", client->GetBrowserTextureBuffer().size());

    int64_t count = 0;
    while (count++ < 5000) {
        CefDoMessageLoopWork();

        if (count == 2500) {
            SYNTHESIS_DEBUG_LOG(std::accumulate(client->GetBrowserTextureBuffer().begin(), client->GetBrowserTextureBuffer().end(), 0ll));
        }
    }

    CefShutdown();

    [delegate release];
    [autopool release]; 

    SYNTHESIS_DEBUG_LOG("Main loop ended.");

    return 0;
}

} // namespace shared
} // namespace synthesis
