#pragma once

#include <filesystem>
#include <fstream>
#include <mutex>
#include <string>

namespace liberty {

class NativeLogger final {
 public:
  static NativeLogger& Instance();

  void Initialize(const std::filesystem::path& log_directory);
  void Info(const std::string& message);
  void Warning(const std::string& message);
  void Error(const std::string& message);

 private:
  void Write(const char* level, const std::string& message);

  std::mutex mutex_;
  std::ofstream stream_;
};

}  // namespace liberty

