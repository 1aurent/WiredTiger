/*-
 * Copyright (c) 2008-2014 WiredTiger, Inc.
 *	All rights reserved.
 *
 * See the file LICENSE for redistribution information.
 */

#include "wt_internal.h"

/*
 * __wt_sleep --
 *	Pause the thread of control.
 */
void
__wt_sleep(uint64_t  seconds, uint64_t  micro_seconds)
{
	Sleep(seconds * 1000 + micro_seconds / 1000);
}
