SOURCE_DIR=$1

export CXXFLAGS="-Wno-error=class-memaccess -Wno-error=ignored-qualifiers -Wno-error=stringop-truncation"
export CFLAGS="-Wno-error=class-memaccess -Wno-error=ignored-qualifiers -Wno-error=stringop-truncation"

make -C ../grpc
