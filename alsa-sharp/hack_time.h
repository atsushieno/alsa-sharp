#ifndef __HACK_TIME_H
#define __HACK_TIME_H

typedef long time_t;

struct timeval {
	time_t		tv_sec;		/* seconds */
	long		tv_usec;	/* microseconds */
};

struct timespec {
	time_t		tv_sec;		/* seconds */
	long		tv_nsec;	/* nanoseconds */
};

#endif

