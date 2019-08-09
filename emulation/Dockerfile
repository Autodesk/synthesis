FROM ubuntu:19.04 AS builder

ENV WPILIB_VERSION v2019.4.1
ENV SYNTHESIS_BRANCH 953-java-emulation
ENV GOPATH /go
ENV GO111MODULE=on
ENV PATH /usr/local/go/bin:$GOPATH/bin:$PATH
ENV ENTER_HEL cd $GOPATH/src/github.com/autodesk/synthesis/hel

RUN mkdir /go

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
    unzip && \
    rm -rf /var/lib/apt/lists/*

RUN cd /root && \
    git clone https://github.com/protocolbuffers/protobuf.git --branch v3.7.0 && \
    cd protobuf && \
    git submodule update --init --recursive && \
    ./autogen.sh && \
    ./configure && \
    make -j12 && make check && \
    make install && \
    ldconfig

RUN curl -O https://storage.googleapis.com/golang/go1.12.7.linux-amd64.tar.gz && \
    tar -xvf go1.12.7.linux-amd64.tar.gz && \
    mv go /usr/local

RUN mkdir -p $GOPATH/src/github.com/autodesk && \ 
    cd $GOPATH/src/github.com/autodesk/ && \
    git clone https://github.com/Autodesk/synthesis.git --branch ${SYNTHESIS_BRANCH} --depth=1

ENV ENTER_HEL cd $GOPATH/src/github.com/autodesk/synthesis/emulation/hel

RUN ${ENTER_HEL} && \
    cmake . -DARCH=X86_64 -DWPILIB=${WPILIB_VERSION} && \
    make hel -j12

RUN ${ENTER_HEL}/../wpi_cross && \
    git clone https://github.com/wpilibsuite/allwpilib --branch ${WPILIB_VERSION} --depth=1 && \
    cd allwpilib && \
    rm -rf hal/src/main/native/sim && \
    cp ../../hel/lib/ni-libraries/src/ni-libraries/src/include hal/src/main/native/ -r && \
    cp wpiutil/src/main/native/include hal/src/main/native/ -r && \
    mv hal/src/main/native/athena hal/src/main/native/sim && \
    cat ../shim_hal.cpp >> hal/src/main/native/sim/HAL.cpp && \
    cat ../shim_jni.cpp >> hal/src/main/native/cpp/jni/HALUtil.cpp && \
    ./gradlew halBaseReleaseSharedLibrary halJNISharedReleaseSharedLibrary :wpilibj:assemble && \
    (./gradlew wpilibj:build || true) 


RUN go get github.com/golang/protobuf/protoc-gen-go
RUN find / -name protoc-gen-go
RUN cd ${GOPATH}/src/github.com/autodesk/synthesis/emulation/bridge && \
    mkdir -p pkg/api/v1 && \
    make build-protobuf && \
    go build -o bridge ./cmd/main.go && \
    cp ./bridge ~

#A hack but it works

FROM ubuntu:19.04

ENV WPILIB_VERSION v2019.4.1
ENV SYNTHESIS_BRANCH 953-java-emulation

# WPILib ignores most gradle tasks if you don't have the toolchain installed
RUN apt-get update && apt-get install -y --no-install-recommends \
    openjdk-11-jre \
    qemu \
    qemu-system-arm \
    openssh-server \
    curl \
    wget \
    unzip && \
    rm -rf /var/lib/apt/lists/*

RUN sed -i '#s/PermitRootLogin\ prohibit-password/PermitRootLogin\ yes/' /etc/ssh/sshd_config && \
    sed -i 's/#PasswordAuthentication/PasswordAuthentication/' /etc/ssh/sshd_config && \
    sed -i 's/#PermitEmptyPasswords\ no/PermitEmptyPasswords\ yes/' /etc/ssh/sshd_config && \ 
    sed -i 's/#Port\ 22/Port\ 10023/' /etc/ssh/sshd_config && \
    sed -i 's/pam_unix.so nullok_secure/pam_unix.so nullok/' /etc/pam.d/common-auth

RUN adduser --disabled-password --gecos "" lvuser && \
    passwd -d lvuser

WORKDIR /home/lvuser

COPY --from=builder /root /home/lvuser
COPY --from=builder /go/src/github.com/autodesk /home/lvuser

RUN ls /home/lvuser/synthesis

RUN pwd
RUN cd protobuf make install && ldconfig

RUN mkdir scripts && \
    cp synthesis/emulation/hel/scripts/download_vm.sh ./scripts && \
    cp synthesis/emulation/hel/scripts/docker_runner.sh ./scripts && \
    cp synthesis/emulation/hel/run_vm.sh . && \
    scripts/download_vm.sh 

RUN find . -regex ".*\.\(so\|jar\)\(\.[0-9]*\)*" -exec mv {} . \;

RUN cd /home/lvuser && \
    ln -s libhel.so /lib/libFRC_NetworkCommunication.so.19.0.0 && \
    ln -s libhel.so /lib/libNiFpga.18.0.0 && \
    ln -s libhel.so /lib/libniriodevenum.so.18.0.0 && \
    ln -s libhel.so /lib/libniriosessions.so.18.0.0 && \
    ln -s libhel.so /lib/libvisa.so && \
    ln -s libhel.so /lib/libNiRioSrv.so.19.0.0 && \
    ls && \
    mv -n ./*.so* /lib

RUN rm -rf ./protobuf && \
    rm -rf ./synthesis

EXPOSE 50051
EXPOSE 10022
EXPOSE 10023

RUN mkdir /run/sshd

CMD /usr/sbin/sshd && \
    nohup ./run_vm.sh && \
    scripts/docker_runner.sh
