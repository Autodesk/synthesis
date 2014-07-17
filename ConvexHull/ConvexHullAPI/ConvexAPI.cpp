// This is the main DLL file.

#include "ConvexAPI.h"

namespace ConvexAPI {
	ConvexHullResult ^iConvexDecomposition::getConvexHullResult(NxU32 hullIndex){
		CONVEX_DECOMPOSITION::ConvexHullResult *result = (CONVEX_DECOMPOSITION::ConvexHullResult*) malloc(sizeof(CONVEX_DECOMPOSITION::ConvexHullResult));
		if (backing->getConvexHullResult(hullIndex, *result)){
			return gcnew ConvexHullResult(result);
		}
		return nullptr;
	}
}