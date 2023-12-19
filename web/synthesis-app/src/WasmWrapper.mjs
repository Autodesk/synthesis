import { useEffect, useState } from 'react';
import createModule from './emsc/synthesis-web.mjs';

// Code to keep for reference
// 
// var makeObj = Module.cwrap("make_obj", "number", null);
// var getObjValue = Module.cwrap("get_obj_value", "number", ["number"]);
// var freeObj = Module.cwrap("free_obj", "void", ["number"]);
// var obj = makeObj();
// var val = getObjValue(obj);
// freeObj(obj);

// console.log(val);

// var protoTest = Module.cwrap("proto_test", "number", ["number", "number"]);

/* Load mira files

const get_file_array = (file) => {
    return new Promise((acc, err) => {
        const reader = new FileReader();
        reader.onload = (event) => { acc(event.target.result) };
        reader.onerror = (err)  => { err(err) };
        console.log(file);
        reader.readAsArrayBuffer(file);
    });
}
const miraFile = await fetch("TestCube_v1.mira", {cache: "no-store"}).then((x) => x.blob());
const temp = await get_file_array(miraFile);
console.log(temp);
const fileb = new Uint8Array(temp);

*/

function wrapParseAssembly(Module) {
    return function (fileb) {

        const wrapFunc = Module.cwrap("parse_assembly", "number", ["number", "number"]);
    
        var ptr = Module._malloc(fileb.length);
        for (var i = 0; i < fileb.length; i++) {
            Module.HEAPU8[ptr + i] = fileb[i];
        }
    
        var res = wrapFunc(ptr, fileb.length);
    
        Module._free(ptr);

        return res;
    };
}

class WasmWrapper {

    #_parseAssembly = () => { };
    #_destroyAssembly = () => { };
    #_debugPrintAssembly = () => { };
    #_testPhys = () => { };

    constructor() {
        this.wrapperPromise = createModule().then((Module) => {
            this.#_parseAssembly = wrapParseAssembly(Module);
            this.#_destroyAssembly = Module.cwrap("destroy_assembly", "void", ["number"]);
            this.#_debugPrintAssembly = Module.cwrap("debug_print_assembly", "void", ["number"]);
            this.#_testPhys = Module.cwrap("test_phys", "void", null);
        });
    }

    parseAssembly(fileB) {
        console.log("Calling Parse Assembly");
        return this.#_parseAssembly(fileB);
    }

    destroyAssembly(assembly) {
        console.log("Calling Destroy Assembly");
        this.#_destroyAssembly(assembly);
    }

    debugPrintAssembly(assembly) {
        console.log("Calling Debug Print Assembly")
        this.#_debugPrintAssembly(assembly);
    }

    testPhys() {
        console.log("Calling Test Phys");
        this.#_testPhys();
        console.log("Physics Test Completed");
    }

}

export var wasmWrapper = new WasmWrapper();
