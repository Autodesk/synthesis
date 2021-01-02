FROM ubuntu:21.04 AS builder

ENV WPILIB_VERSION v2020.3.2
ENV SYNTHESIS_BRANCH emulator-restore
ENV TEMPPATH /temp
ENV PATH $TEMPPATH/bin:$PATH

RUN mkdir $TEMPPATH

# This avoids issues with an interactive configuration step for tzdata
RUN DEBIAN_FRONTEND="noninteractive" apt-get update && apt-get install -y --no-install-recommends tzdata

# WPILib ignores most gradle tasks if you don't have the toolchain installed
RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc \
    g++ \
    git \
    make \
    zlib1g-dev \ 
    automake \
    libtool \
    gdb-multiarch \
    gdbserver \
    openjdk-11-jdk \
    openjdk-11-jre \
    cmake \
    curl \
    unzip \
    patch && \
    rm -rf /var/lib/apt/lists/*

RUN cd /root && \
    git clone https://github.com/protocolbuffers/protobuf.git --branch v3.7.0 --depth=1 && \
    cd protobuf && \
    git submodule update --init --recursive && \
    ./autogen.sh && \
    ./configure && \
    make -j12 && \
    make check && \
    make install && \
    ldconfig

RUN mkdir -p $TEMPPATH/src/github.com/autodesk && \ 
    cd $TEMPPATH/src/github.com/autodesk/ && \
    git clone https://github.com/Autodesk/synthesis.git --branch ${SYNTHESIS_BRANCH} --depth=1

ENV ENTER_HEL cd $TEMPPATH/src/github.com/autodesk/synthesis/emulation/hel

RUN ${ENTER_HEL} && \
    mkdir build && \
    cd build && \
    cmake .. -DX86=ON -DWPILIB_VERSION=${WPILIB_VERSION} && \
    make hel

RUN ${ENTER_HEL}/../java_vm/wpi_cross && \
    git clone https://github.com/wpilibsuite/allwpilib --branch ${WPILIB_VERSION} --depth=1 && \
    cd allwpilib && \
    rm -rf hal/src/main/native/sim && \
    cp ../../../hel/build/lib/ni-libraries/src/ni-libraries/src/include hal/src/main/native/ -r && \
    cp wpiutil/src/main/native/include hal/src/main/native/ -r && \
    mv hal/src/main/native/athena hal/src/main/native/sim && \
    cat ../shim_hal.cpp >> hal/src/main/native/sim/HAL.cpp && \
    cat ../shim_jni.cpp >> hal/src/main/native/cpp/jni/HALUtil.cpp && \
    patch -p0 < ../AddressableLED.patch && \
    patch -p0 < ../FRCDriverStation.patch && \
    ./gradlew halBaseLinuxx86-64ReleaseSharedLibrary halJNILinuxx86-64ReleaseSharedLibrary :wpilibj:assemble && \
    (./gradlew wpilibj:build || true)  # This gradle build step will fail, but that's okay 

RUN mkdir $TEMPPATH/to_copy && \
    ${ENTER_HEL}/../ && \
    find . -regex ".*\.\(so\)\(\.[0-9]*\)*" -exec mv {} $TEMPPATH/to_copy \; && \
    cp hel/scripts/frc_program_runner.sh $TEMPPATH/to_copy && \
    cp hel/scripts/docker_runner.sh $TEMPPATH/to_copy

#A hack but it works

FROM ubuntu:21.04

ENV WPILIB_VERSION v2020.3.2
ENV SYNTHESIS_BRANCH emulator-restore
ENV TEMPPATH /temp

RUN DEBIAN_FRONTEND="noninteractive" apt-get update && apt-get install -y --no-install-recommends tzdata

# WPILib ignores most gradle tasks if you don't have the toolchain installed
RUN apt-get update && apt-get install -y --no-install-recommends \
    openjdk-11-jre \
    openssh-server \
    vim && \
    rm -rf /var/lib/apt/lists/*

RUN sed -i '#s/PermitRootLogin\ prohibit-password/PermitRootLogin\ yes/' /etc/ssh/sshd_config && \
    sed -i 's/#PasswordAuthentication/PasswordAuthentication/' /etc/ssh/sshd_config && \
    sed -i 's/#PermitEmptyPasswords\ no/PermitEmptyPasswords\ yes/' /etc/ssh/sshd_config && \ 
    sed -i 's/#Port\ 22/Port\ 10023/' /etc/ssh/sshd_config && \
    sed -i 's/pam_unix.so nullok_secure/pam_unix.so nullok/' /etc/pam.d/common-auth

RUN adduser --disabled-password --gecos "" lvuser && \
    passwd -d lvuser

WORKDIR /home/lvuser

# Copy synthesis dir
COPY --from=builder $TEMPPATH/to_copy/* /home/lvuser/

RUN  mv -n ./libhel.so \
    ./libwpiHaljni.so \
    ./libwpiHald.so \
    ./libwpiutild.so \
    ./libcscorejni.so \
    ./libcscored.so \
    ./libopencv*.so* \
    ./libntcorejni.so \
    ./libntcored.so \
    /lib/

RUN rm -rf *.so*

EXPOSE 50051
EXPOSE 10022
EXPOSE 10023

RUN mkdir /run/sshd

RUN mkdir /tmp/frc_versions && \
    touch /tmp/frc_versions/FRC_Lib_Version.ini

RUN printf "2.0\n\
v2020.3.2\n\
2020v10-image" > /home/lvuser/.vminfo

CMD /usr/sbin/sshd && \
    while true; do echo ""; done
