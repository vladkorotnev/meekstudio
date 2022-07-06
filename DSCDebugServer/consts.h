#pragma once
#include <cstdint>

/// Script engine tick event
const uint64_t PROCESS_CMD = 0x14011cba0; 

/// Location of script memory
const uint64_t SCPT_START = 0x140CDD978 + 0x10;

/// Running flag of the script engine
const uint64_t SCPT_IS_RUNNING = 0x140CDD978 + 0x8;

/// Script engine program counter
const uint64_t SCPT_COUNTER = 0x140CDD978 + 0x2bf2c;
