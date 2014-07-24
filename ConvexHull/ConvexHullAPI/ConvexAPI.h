// ConvexAPI.h
#include "NvConvexDecomposition.h"
#include "NvStanHull.h"
#include <string.h>
#include <stdio.h>
using namespace System;

namespace ConvexAPI {
	// Represents a single convex hull that has been computed as part of a set.
	public ref class ConvexHullResult {
	private:
		CONVEX_DECOMPOSITION::ConvexHullResult *backing;
	internal:
		ConvexHullResult(CONVEX_DECOMPOSITION::ConvexHullResult *back) {
			backing = back;
		}
	public:
		/// Gets the number of triangles in this result.
		NxU32 getTriangleCount() {
			return backing->mTcount;
		}
		/// Gets the number of verticies in this result.
		NxU32 getVertexCount() {
			return backing->mVcount;
		}
		/// Retrieves a managed array of the verticies in this result.
		cli::array<NxF32> ^getVertices();
		/// Retrieves a managed array of the indicies in this result.  Zero-based
		cli::array<NxU32> ^getIndicies();
	};

	/// Computes a set of convex hulls to represent a concave mesh.
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

		/// Resets this decomposer, preparing it for another convex hull set computation.
		void reset(void) {backing->reset();}

		/// Sets the mesh to compute the convex hulls for.
		/// @param vertCount The number of verticies
		/// @param verts The vertex positions.  3 floats per vertex
		/// @param faceCount The number of triangles
		/// @param facets The index buffer.  3 values per triangle, zero based.
		bool setMesh(NxU32 vertCount, array<NxF32> ^verts, NxU32 faceCount, array<NxU32> ^facets);

		/// Computes the convex decomposition for the currently set mesh.
		NxU32 computeConvexDecomposition() {
			return backing->computeConvexDecomposition(0,8,255,.05,1000,.5,false,false,true);
		}


		/// Computes the convex decomposition for the currently set mesh.
		/// @param skinWidth Skin width on the convex hulls generated
		/// @param decompositionDepth recursion depth for convex decomposition.
		/// @param maxHullVertices maximum number of vertices in output convex hulls.
		/// @param concavityThresholdPercent The percentage of concavity allowed without causing a split to occur.
		/// @param mergeThresholdPercent The percentage of volume difference allowed to merge two convex hulls.
		/// @param volumeSplitThresholdPercent The percentage of the total volume of the object above which splits will still occur.
		/// @param useInitialIslandGeneration whether or not to perform initial island generation on the input mesh.
		/// @param useIslandGeneration Whether or not to perform island generation at each split.  Currently disabled due to bug in RemoveTjunctions
		/// @param useBackgroundThread Should the operation be performed in the background.
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

		/// If building the convex hulls in a background thread, this returns true if it is complete.
		bool isComputeComplete(void){return backing->isComputeComplete();}

		/// Cause background thread computation to abort early.  Will return no results. Use 'isComputeComplete' to confirm the thread is done.
		bool cancelCompute(void){return backing->cancelCompute();}

		// Returns the number of convex hulls produced.
		NxU32 getHullCount(void){return backing->getHullCount();}
		
		// Returns each convex hull result.
		ConvexHullResult ^getConvexHullResult(NxU32 hullIndex);
	};

	/// Computes a single convex hull for a point cloud
	public ref class StandaloneConvexHull {
	private:
		CONVEX_DECOMPOSITION::HullLibrary *library;
		CONVEX_DECOMPOSITION::HullResult *backing;
	public:
		StandaloneConvexHull(){
			backing = new CONVEX_DECOMPOSITION::HullResult();
			library = new CONVEX_DECOMPOSITION::HullLibrary();
		}
		~StandaloneConvexHull(){
			if (backing != NULL){
				library->ReleaseResult(*backing);
				delete backing;
				backing = NULL;
			}
			delete library;
		}
		/// Computers a convex hull for the given point cloud.
		/// @param vertCount The number of verticies
		/// @param verts The vertex positions.  3 floats per vertex
		void computeFor(NxU32 vertCount, array<NxF32> ^verts);

		/// Gets the number of triangles in this result.
		NxU32 getTriangleCount() {
			return backing->mNumFaces;
		}
		/// Gets the number of verticies in this result.
		NxU32 getVertexCount() {
			return backing->mNumOutputVertices;
		}
		/// Retrieves a managed array of the verticies in this result.
		cli::array<NxF32> ^getVertices();
		/// Retrieves a managed array of the indicies in this result.  Zero-based
		cli::array<NxU32> ^getIndicies();
	};
}; // end of namespace
