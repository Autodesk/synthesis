/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2015. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "DsClient.h"

#include "llvm/raw_ostream.h"
#include "llvm/SmallString.h"
#include "support/raw_socket_istream.h"
#include "tcpsockets/TCPConnector.h"

#include "Dispatcher.h"
#include "Log.h"

using namespace nt;

ATOMIC_STATIC_INIT(DsClient)

class DsClient::Thread : public wpi::SafeThread {
 public:
  Thread(unsigned int port) : m_port(port) {}

  void Main();

  unsigned int m_port;
  std::unique_ptr<wpi::NetworkStream> m_stream;
};

void DsClient::Start(unsigned int port) {
  auto thr = m_owner.GetThread();
  if (!thr)
    m_owner.Start(new Thread(port));
  else
    thr->m_port = port;
}

void DsClient::Stop() {
  {
    // Close the stream so the read (if any) terminates.
    auto thr = m_owner.GetThread();
    if (thr) {
      thr->m_active = false;
      if (thr->m_stream) thr->m_stream->close();
    }
  }
  m_owner.Stop();
}

void DsClient::Thread::Main() {
  unsigned int oldip = 0;
  wpi::Logger nolog;  // to silence log messages from TCPConnector

  while (m_active) {
    // wait for periodic reconnect or termination
    auto timeout_time =
        std::chrono::steady_clock::now() + std::chrono::milliseconds(500);
    unsigned int port;
    {
      std::unique_lock<std::mutex> lock(m_mutex);
      m_cond.wait_until(lock, timeout_time, [&] { return !m_active; });
      port = m_port;
    }
    if (!m_active) goto done;

    // Try to connect to DS on the local machine
    m_stream =
        wpi::TCPConnector::connect("127.0.0.1", 1742, nolog, 1);
    if (!m_active) goto done;
    if (!m_stream) continue;

    DEBUG3("connected to DS");
    wpi::raw_socket_istream is(*m_stream);

    while (m_active && !is.has_error()) {
      // Read JSON "{...}".  This is very limited, does not handle quoted "}" or
      // nested {}, but is sufficient for this purpose.
      llvm::SmallString<128> json;
      char ch;

      // Throw away characters until {
      do {
        is.read(ch);
        if (is.has_error()) break;
        if (!m_active) goto done;
      } while (ch != '{');
      json += '{';

      if (is.has_error()) {
        m_stream = nullptr;
        break;
      }

      // Read characters until }
      do {
        is.read(ch);
        if (is.has_error()) break;
        if (!m_active) goto done;
        json += ch;
      } while (ch != '}');

      if (is.has_error()) {
        m_stream = nullptr;
        break;
      }
      DEBUG3("json=" << json);

      // Look for "robotIP":12345, and get 12345 portion
      size_t pos = json.find("\"robotIP\"");
      if (pos == llvm::StringRef::npos) continue;  // could not find?
      pos += 9;
      pos = json.find(':', pos);
      if (pos == llvm::StringRef::npos) continue;  // could not find?
      size_t endpos = json.find_first_not_of("0123456789", pos + 1);
      DEBUG3("found robotIP=" << json.slice(pos + 1, endpos));

      // Parse into number
      unsigned int ip;
      if (json.slice(pos + 1, endpos).getAsInteger(10, ip)) continue;  // error

      // If zero, clear the server override
      if (ip == 0) {
        Dispatcher::GetInstance().ClearServerOverride();
        oldip = 0;
        continue;
      }

      // If unchanged, don't reconnect
      if (ip == oldip) continue;
      oldip = ip;

      // Convert number into dotted quad
      json.clear();
      llvm::raw_svector_ostream os{json};
      os << ((ip >> 24) & 0xff) << "." << ((ip >> 16) & 0xff) << "."
         << ((ip >> 8) & 0xff) << "." << (ip & 0xff);
      INFO("client: DS overriding server IP to " << os.str());
      Dispatcher::GetInstance().SetServerOverride(json.c_str(), port);
    }

    // We disconnected from the DS, clear the server override
    Dispatcher::GetInstance().ClearServerOverride();
    oldip = 0;
  }

done:
  Dispatcher::GetInstance().ClearServerOverride();
}
