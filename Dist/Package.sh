#!/bin/sh
set -e
targetDir="packaged_output"
cd "$(dirname "$0")/.."
rm -fr "$targetDir" && mkdir -p "$targetDir"
find "output" -type f \( -wholename "output/EmuHawk.exe" -o -wholename "output/DiscoHawk.exe" -o -wholename "output/*.config" -o -wholename "output/defctrl.json" -o -wholename "output/EmuHawkMono.sh" -o -wholename "output/dll/*" -o -wholename "output/Shaders/*" -o -wholename "output/gamedb/*" -o -wholename "output/Tools/*" -o -wholename "output/NES/Palettes/*" -o -wholename "output/Lua/*" -o -wholename "output/Gameboy/Palettes/*" -o -wholename "output/overlay/*" \) \
	-not -name "*.pdb" -not -name "*.lib" -not -name "*.pgd" -not -name "*.ipdb" -not -name "*.iobj" -not -name "*.exp" -not -name "*.ilk" \
	-not -wholename "output/dll/*.xml" -not -wholename "output/dll/libsneshawk-64*.exe" -not -wholename "output/dll/gpgx.elf" -not -wholename "output/dll/miniclient.*" \
	-exec install -D -m644 "{}" "packaged_{}" \;
cd "$targetDir"
if [ "$1" = "windows-x64" ]; then
	rm -f "EmuHawkMono.sh"
	cd "dll"
	rm -f "libe_sqlite3.so" "libSDL2.so" "OpenTK.dll.config" \
		"libbizlynx.dll.so" "libbizswan.dll.so" "libblip_buf.so" "libbizhash.so" "libdarm.so" "libemu83.so" "libencore.so" "libfwunpack.so" "libgambatte.so" "libLibretroBridge.so" "libquicknes.so" "librcheevos.so" "libsameboy.so" "libmgba.dll.so" "libMSXHawk.so" "libwaterboxhost.so"
else
	find . -type f -name "*.sh" -exec chmod +x {} \; # installed with -m644 but needs to be 755
	cd "dll"
	rm -f "e_sqlite3.dll" "lua54.dll" "SDL.dll" "SDL2.dll" \
		"mupen64plus-audio-bkm.dll" "mupen64plus-input-bkm.dll" "mupen64plus-rsp-cxd4-sse2.dll" "mupen64plus-rsp-hle.dll" "mupen64plus-video-angrylion-rdp.dll" "mupen64plus-video-glide64.dll" "mupen64plus-video-glide64mk2.dll" "mupen64plus-video-GLideN64.dll" "mupen64plus-video-rice.dll" "mupen64plus.dll" "octoshock.dll" \
		"bizlynx.dll" "bizswan.dll" "blip_buf.dll" "libbizhash.dll" "libdarm.dll" "libemu83.dll" "libfwunpack.dll" "libgambatte.dll" "libLibretroBridge.dll" "libquicknes.dll" "librcheevos.dll" "libsameboy.dll" "mgba.dll" "MSXHawk.dll" "waterboxhost.dll" "encore.dll"
fi
