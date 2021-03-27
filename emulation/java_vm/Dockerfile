# Builder Docker build stage

FROM ubuntu:18.04 AS builder

ENV WPILIB_VERSION v2021.2.2
ENV SYNTHESIS_BRANCH emulator-restore
ENV TEMPPATH /temp
ENV PATH $TEMPPATH/bin:$PATH

RUN mkdir $TEMPPATH

RUN apt-get update && apt-get upgrade -y

# This avoids issues with an interactive configuration step for tzdata
RUN DEBIAN_FRONTEND="noninteractive" apt-get install -y --no-install-recommends tzdata

RUN apt-get install -y --no-install-recommends \
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
    patch

# Build and install protobuf
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

# Build Synthesis
RUN mkdir -p $TEMPPATH/src/github.com/autodesk && \ 
    cd $TEMPPATH/src/github.com/autodesk/ && \
    git clone https://github.com/Autodesk/synthesis.git --branch ${SYNTHESIS_BRANCH} --depth=1

ENV ENTER_HEL cd $TEMPPATH/src/github.com/autodesk/synthesis/emulation/hel

RUN ${ENTER_HEL} && \
    mkdir build && \
    cd build && \
    cmake .. -DX86=ON -DWPILIB_VERSION=${WPILIB_VERSION} && \
    make hel

# Build WPILibj
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

# Move needed files into one location
RUN mkdir $TEMPPATH/to_copy && \
    ${ENTER_HEL}/../ && \
    find . -regex ".*\.\(so\)\(\.[0-9]*\)*" -exec mv {} $TEMPPATH/to_copy \; && \
    mv hel/scripts/frc_program_runner.sh $TEMPPATH/to_copy && \
    mv hel/scripts/docker_runner.sh $TEMPPATH/to_copy

# Final build Docker build stage

FROM ubuntu:18.04

ENV WPILIB_VERSION v2020.3.2
ENV SYNTHESIS_BRANCH emulator-restore
ENV TEMPPATH /temp

RUN echo 'root:root' | chpasswd

RUN apt-get -y update

RUN DEBIAN_FRONTEND="noninteractive" apt-get install -y --no-install-recommends tzdata

RUN apt-get -y install --no-install-recommends \
    linux-image-generic:amd64 \
    systemd-sysv \
    dbus \
    openjdk-11-jre \
    openssh-server \
    vim \
    iputils-ping \
    iproute2 \
    isc-dhcp-client \
    network-manager \
    ifupdown2

RUN adduser --disabled-password --gecos "" lvuser && \
    passwd -d lvuser

WORKDIR /home/lvuser

RUN sed -i '#s/PermitRootLogin\ prohibit-password/PermitRootLogin\ yes/' /etc/ssh/sshd_config && \
    sed -i 's/#PasswordAuthentication/PasswordAuthentication/' /etc/ssh/sshd_config && \
    sed -i 's/#PermitEmptyPasswords\ no/PermitEmptyPasswords\ yes/' /etc/ssh/sshd_config && \ 
    sed -i 's/pam_unix.so nullok_secure/pam_unix.so nullok/' /etc/pam.d/common-auth

RUN printf "auto enp0s3\n\
allow-hotplug enp0s3\n\
iface enp0s3 inet dhcp" >> /etc/network/interfaces

EXPOSE 22
EXPOSE 50051

RUN mkdir /run/sshd # TODO is this necessary?

# Copy synthesis dir
COPY --from=builder $TEMPPATH/to_copy/* /home/lvuser/

# Move WPILib libraries into /lib/
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

# Touch WPILib version file used by wpilibj
RUN mkdir /tmp/frc_versions && \
    touch /tmp/frc_versions/FRC_Lib_Version.ini

# Write .vminfo for HEL
RUN printf "2.0\n\
v2020.3.2\n\
2020v10-image" > /home/lvuser/.vminfo

# Login as lvuser automatically
RUN mkdir -p /etc/systemd/system/getty@tty1.service.d/ && \
	printf "[Service]\n\
ExecStart=\n\
ExecStart=/usr/sbin/agetty --autologin lvuser --noclear %%I $TERM" > /etc/systemd/system/getty@tty1.service.d/override.conf

# Remove apt lists to reclaim space
RUN rm -rf /var/lib/apt/lists/*

CMD /usr/sbin/sshd && \
    while true; do echo ""; done

