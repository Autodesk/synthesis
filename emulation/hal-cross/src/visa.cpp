#include "visa/visa.h"

#define HEL_VI_STATUS_VALUE {}

ViStatus _VI_FUNC viOpenDefaultRM(ViPSession vi){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viFindRsrc(ViSession sesn, ViString expr, ViPFindList vi, ViPUInt32 retCnt, ViChar _VI_FAR desc[]){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viFindNext(ViFindList vi, ViChar _VI_FAR desc[]){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viOpen(ViSession sesn, ViRsrc name, ViAccessMode mode, ViUInt32 timeout, ViPSession vi){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viClose(ViObject vi){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viSetAttribute(ViObject vi, ViAttr attrName, ViAttrState attrValue){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viGetAttribute(ViObject vi, ViAttr attrName, void _VI_PTR attrValue){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viRead(ViSession vi, ViPBuf buf, ViUInt32 cnt, ViPUInt32 retCnt){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viWrite(ViSession vi, ViBuf buf, ViUInt32 cnt, ViPUInt32 retCnt){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viClear(ViSession vi){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viSetBuf(ViSession vi, ViUInt16 mask, ViUInt32 size){
	return HEL_VI_STATUS_VALUE;
}

ViStatus _VI_FUNC viFlush(ViSession vi, ViUInt16 mask){
	return HEL_VI_STATUS_VALUE;
}
