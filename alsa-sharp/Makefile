
NCLANG = _SPECIFY_NCLANG_ENV_VARIABLE_TO_YOUR_PATH_TO_NCLANG_CHECKOUT_
PINVOKEGEN = $(NCLANG)/samples/PInvokeGenerator/bin/Debug/PInvokeGenerator.exe

all: alsa-interop.cs
	mono --debug $(PINVOKEGEN) \
		--out:alsa-interop.cs --ns:AlsaSharp --lib:asound \
		--match:alsa --match:hack_time.h \
		hack_time.h \
		/usr/include/alsa/asoundlib.h \
		--arg:"-Dsize_t=unsigned int" \
		--arg:"-Dssize_t=int" \
		--arg:"-Dtime_t=long" \
		--arg:"-Dpid_t=int" \
		--arg:"-Dsnd_seq_event_type_t=unsigned char" \
		--arg:"-Dsnd_seq_tick_time_t=unsigned int" \
		--arg:"-Dsnd_pcm_sframes_t=long" \
		--arg:"-Dsnd_pcm_uframes_t=unsigned long" \
		--arg:-Dsnd_config_iterator_t=void* \
		--arg:"-Du_int8_t=unsigned char" \
		--arg:"-Du_int16_t=unsigned short" \
		--arg:"-Du_int32_t=unsigned int" \
		--arg:"-Du_int64_t=unsigned long" \
		#
