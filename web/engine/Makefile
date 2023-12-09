.PHONY: all clean

exported_symbols = '["_make_obj", "_get_obj_value", "_free_obj", "_malloc", "_free"]'

all: out/test.mjs
	mv out/test.mjs ../synthesis-app/src/emsc/test.mjs
	mv out/test.wasm ../synthesis-app/public/test.wasm

out/test.mjs: src/test.cpp
	em++ --no-entry src/test.cpp -o out/test.mjs  \
	  --pre-js js/locateFile.js  \
	  -s ENVIRONMENT='web'  \
	  -s EXPORT_NAME='createModule'  \
	  -s USE_ES6_IMPORT_META=0  \
	  -s EXPORTED_FUNCTIONS=$(exported_symbols)  \
	  -s EXPORTED_RUNTIME_METHODS='["ccall", "cwrap"]'

clean:
	rm ../synthesis-app/src/emsc/* ../synthesis-app/public/*.wasm