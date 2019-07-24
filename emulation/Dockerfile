FROM ubuntu:19.04

ENV WPILIB_VERSION v2019.4.1
ENV SYNTHESIS_BRANCH 953-java-emulation
ENV ENTER_HEL cd /home/lvuser/synthesis/emulation/hel

# WPILib ignores most gradle tasks if you don't have the toolchain installed
RUN apt-get update && apt-get install -y \
    gcc \
    g++ \
    git \
    vim \
    gdb \
    automake \
    libtool \
    gdb-multiarch \
    gdbserver \
    openjdk-11-jdk \
    openjdk-11-jre \
    qemu \
    qemu-system-arm \
    openssh-server \ 
    cmake \
    sudo \
    zip \
    unzip

RUN sed -i '#s/PermitRootLogin\ prohibit-password/PermitRootLogin\ yes/' /etc/ssh/sshd_config && \
    sed -i 's/#PasswordAuthentication/PasswordAuthentication/' /etc/ssh/sshd_config && \
    sed -i 's/#PermitEmptyPasswords\ no/PermitEmptyPasswords\ yes/' /etc/ssh/sshd_config && \ 
    sed -i 's/#Port\ 22/Port\ 10023/' /etc/ssh/sshd_config

RUN adduser --disabled-password --gecos "" lvuser && \
    usermod -aG sudo lvuser

RUN sed -i 's/root\ ALL=(ALL)\ ALL/&\nlvuser\ ALL=(ALL)\ NOPASSWD:ALL/' /etc/sudoers

RUN git config --global user.email "no-reply@gmail.com"
RUN git config --global user.name "no-reply"

RUN cd /home/lvuser && \
    git clone https://github.com/protocolbuffers/protobuf.git --branch v3.7.0 && \
    cd protobuf && \
    git submodule update --init --recursive && \
    ./autogen.sh && \
    ./configure "CFLAGS=-fPIC" "CXXFLAGS=-fPIC" && \
    make -j12 && make check && \
    sudo make install && \
    sudo ldconfig

RUN echo "test"
RUN echo "test"

RUN cd /home/lvuser && \
    git clone https://github.com/Autodesk/synthesis.git --branch ${SYNTHESIS_BRANCH} --depth=1

RUN ${ENTER_HEL} && \
    cmake . -DARCH=X86_64 -DWPILIB=${WPILIB_VERSION} && \
    make hel -j12

RUN ${ENTER_HEL} && \
    ./scripts/download_vm.sh 

RUN ${ENTER_HEL}/../wpi_cross && \
    git clone https://github.com/wpilibsuite/allwpilib --branch ${WPILIB_VERSION} --depth=1 && \
    cd allwpilib && \
    rm -rf hal/src/main/native/sim && \
    cp ../../hel/lib/ni-libraries/src/ni-libraries/src/include hal/src/main/native/ -r && \
    cp wpiutil/src/main/native/include hal/src/main/native/ -r && \
    mv hal/src/main/native/athena hal/src/main/native/sim && \
    cat ../shim.cpp >> hal/src/main/native/cpp/jni/HALUtil.cpp && \
    ./gradlew halBaseReleaseSharedLibrary halJNISharedReleaseSharedLibrary :wpilibj:assemble

RUN ${ENTER_HEL}/../ && \
    find . -regex ".*\.\(so\|jar\)\([0-9]*\)*" -exec cp {} /home/lvuser \;

RUN cd /home/lvuser && \
    ln -s libhel.so /lib/libFRC_NetworkCommunication.so.19.0.0 && \
    ln -s libhel.so /lib/libNiFpga.18.0.0 && \
    ln -s libhel.so /lib/libniriodevenum.so.18.0.0 && \
    ln -s libhel.so /lib/libniriosessions.so.18.0.0 && \
    ln -s libhel.so /lib/libvisa.so && \
    ln -s libhel.so /lib/libNiRioSrv.so.19.0.0 && \
    ls && \
    mv -n ./*.so* /lib

EXPOSE 50051
EXPOSE 10022
EXPOSE 10023

RUN mkdir /run/sshd

CMD /usr/sbin/sshd && \
    ${ENTER_HEL} && \ 
    nohup /home/lvuser/synthesis/emulation/hel/run_vm.sh && \
    tail -f /dev/null