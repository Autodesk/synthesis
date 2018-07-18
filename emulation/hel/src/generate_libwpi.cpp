#include <fstream>

int main(int argc, char** argv) {
    std::ofstream f;
    f.open("lib/libwpi.so");
    if (argc < 2) {
        f <<""
            "OUTPUT_FORMAT(elf32-littlearm)\n" <<
            "GROUP ( AS_NEEDED ( libcscore.so libntcore.so libwpiutil.so libniriosession.so.17.0.0 libniriodevenum.so.17.0.0 libRoboRIO_FRC_ChipObject.so.18.0.0 libvisa.so libNiRioSrv.so.17.0.0 libFRC_NetworkCommunication.so.18.0.0 libNiFpga.so.17.0.0 libNiFpgaLv.so.17.0.0 libwpiHal.so libwpilibc.so ) )\n";
    } else {
        f << "GROUP ( AS_NEEDED ( libcscore.so libntcore.so libwpiutil.so libwpiHal.so libwpilibc.so ) )\n";
    }
}
