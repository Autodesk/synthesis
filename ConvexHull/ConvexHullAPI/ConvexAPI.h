// ConvexAPI.h
#include "NvConvexDecomposition.h"
#include <string.h>

using namespace System;

namespace ConvexAPI {
	public ref class ConvexHullResult {
	private:
		CONVEX_DECOMPOSITION::ConvexHullResult *backing;
	internal:
		ConvexHullResult(CONVEX_DECOMPOSITION::ConvexHullResult *back) {
			backing = back;
		}
	public:
		NxU32 getTriangleCount() {
			return backing->mTcount;
		}
		NxU32 getVertexCount() {
			return backing->mVcount;
		}
		cli::array<NxF32> ^getVertices(){
			cli::array<NxF32> ^verts = gcnew cli::array<NxF32>(backing->mVcount * 3);
			System::Runtime::InteropServices::Marshal::Copy(IntPtr((void*)backing->mVertices),verts,0,verts->Length);
			return verts;
		}
		cli::array<NxU32> ^getIndicies(){
			cli::array<NxU32> ^verts = gcnew cli::array<NxU32>(backing->mTcount * 3);
			pin_ptr<NxU32> ptr = &verts[0];
			int cpyCount = backing->mTcount * 3 * sizeof(NxU32);
			memcpy_s(ptr, cpyCount, backing->mIndices, cpyCount);
			return verts;
		}
	};

	public ref class iConvexDecomposition
	{
	internal:
		CONVEX_DECOMPOSITION::iConvexDecomposition *backing;
	public:
		iConvexDecomposition() {
			backing = CONVEX_DECOMPOSITION::createConvexDecomposition();
		}
		virtual ~iConvexDecomposition(void)
		{
			CONVEX_DECOMPOSITION::releaseConvexDecomposition(backing);
		}

		void reset(void) {backing->reset();}

		bool addTriangle(array<NxF32> ^p1,array<NxF32> ^p2,array<NxF32> ^p3)
		{
			pin_ptr<NxF32> p1p = &p1[0];
			pin_ptr<NxF32> p2p = &p2[0];
			pin_ptr<NxF32> p3p = &p3[0];
			return backing->addTriangle(p1p, p2p, p3p);
		}

		NxU32 computeConvexDecomposition() {
			return backing->computeConvexDecomposition();
		}

		// skinWidth =0
		// decompositionDepth=8
		// maxHullVerticies=64
		// concavityThreshPercent = 0.1
		// mergeThresholdPercent = 30
		// volumeSplitThresholdPercent = 0.1
		// useInitialIslandGeneration = true
		// useIslandGeneration = false
		// useBackgroundThread = true
		NxU32 computeConvexDecomposition(NxF32 skinWidth,			// Skin width on the convex hulls generated
			NxU32 decompositionDepth, // recursion depth for convex decomposition.
			NxU32 maxHullVertices,	// maximum number of vertices in output convex hulls.
			NxF32 concavityThresholdPercent, // The percentage of concavity allowed without causing a split to occur.
			NxF32 mergeThresholdPercent,    // The percentage of volume difference allowed to merge two convex hulls.
			NxF32 volumeSplitThresholdPercent, // The percentage of the total volume of the object above which splits will still occur.
			bool  useInitialIslandGeneration,	// whether or not to perform initial island generation on the input mesh.
			bool  useIslandGeneration,		// Whether or not to perform island generation at each split.  Currently disabled due to bug in RemoveTjunctions
			bool  useBackgroundThread)	// Whether or not to compute the convex decomposition in a background thread, the default is true.
		{
			return backing->computeConvexDecomposition(skinWidth, decompositionDepth,maxHullVertices, concavityThresholdPercent, mergeThresholdPercent, volumeSplitThresholdPercent
				,useInitialIslandGeneration,useIslandGeneration,useBackgroundThread);
		};

		bool isComputeComplete(void){return backing->isComputeComplete();} // if building the convex hulls in a background thread, this returns true if it is complete.

		bool cancelCompute(void){return backing->cancelCompute();} // cause background thread computation to abort early.  Will return no results. Use 'isComputeComplete' to confirm the thread is done.


		NxU32 getHullCount(void){return backing->getHullCount();} // returns the number of convex hulls produced.
		// returns each convex hull result.
		ConvexHullResult ^getConvexHullResult(NxU32 hullIndex);
	};


}; // end of namespace
