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

core::Ptr<adsk::fusion::Occurrence> Utility::lowerOccurrence(core::Ptr<fusion::Joint> joint)
{
	if (levelOfOccurrence(joint->occurrenceOne()) >= levelOfOccurrence(joint->occurrenceTwo()))
		return joint->occurrenceOne();
	else
		return joint->occurrenceTwo();
}

core::Ptr<adsk::fusion::Occurrence> Utility::upperOccurrence(core::Ptr<fusion::Joint> joint)
{
	if (levelOfOccurrence(joint->occurrenceOne()) < levelOfOccurrence(joint->occurrenceTwo()))
		return joint->occurrenceOne();
	else
		return joint->occurrenceTwo();
}

std::string Utility::getUniqueJointID(core::Ptr<adsk::fusion::Joint> joint)
{
	return upperOccurrence(joint)->fullPathName() + joint->name();
}
