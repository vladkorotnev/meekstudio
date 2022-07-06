#pragma once
#include "interop.h"

extern void StartFastbootEx();
typedef void DoneBootCallback();
extern DoneBootCallback* onBootCallback;