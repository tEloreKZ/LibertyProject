#include "Logging/NativeLogger.h"

#include <Windows.h>

#include <chrono>
#include <iomanip>
#include <sstream>

namespace liberty {

NativeLogger& NativeLogger::Instance() {
  static NativeLogger logger;
  return logger;
}

void NativeLogger::Initialize(const std::filesystem::path& log_directory) {
  std::scoped_lock lock(mutex_);
  std::error_code error;
  std::filesystem::create_directories(log_directory, error);
  stream_.open(log_directory / L"cef.log", std::ios::out | std::ios::app);
}

void NativeLogger::Info(const std::string& message) { Write("INFO", message); }
void NativeLogger::Warning(const std::string& message) { Write("WARN", message); }
void NativeLogger::Error(const std::string& message) { Write("ERROR", message); }

void NativeLogger::Write(const char* level, const std::string& message) {
  const auto now = std::chrono::system_clock::now();
  const auto time = std::chrono::system_clock::to_time_t(now);
  std::tm local{};
  localtime_s(&local, &time);
  std::ostringstream line;
  line << '[' << std::put_time(&local, "%Y-%m-%d %H:%M:%S") << "] [CEF] ["
       << level << "] " << message << '\n';
  const auto value = line.str();
  std::scoped_lock lock(mutex_);
  if (stream_.is_open()) {
    stream_ << value;
    stream_.flush();
  }
  OutputDebugStringA(value.c_str());
}

}  // namespace liberty

