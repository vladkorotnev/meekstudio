#pragma once

extern void EnableVfs();
extern void SetVfsPvDb(char* basePath, char* audioPath, char* dscPath, char* gmPvLstPath);

typedef struct vfs_info_struct {
	char* pvDbPath;
	char* dscPath;
	char* audioPath;
	char* pvLstFarc;
} VfsInfo;