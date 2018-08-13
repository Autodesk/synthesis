#include "Utility.h"
#include <Fusion/Components/Occurrence.h>
#include <Fusion/Components/Joint.h>

using namespace adsk;
using namespace BXDJ;

int Utility::levelOfOccurrence(core::Ptr<fusion::Occurrence> occurrence)
{
	if (occurrence == nullptr)
		return -1;

	return levelOfOccurrence(occurrence->assemblyContext()) + 1;
}

std::string Utility::getUniqueJointID(core::Ptr<adsk::fusion::Joint> joint)
{
	return joint->occurrenceTwo()->fullPathName() + joint->name();
}
