#include "Utility.h"

using namespace adsk;
using namespace BXDJ;

int Utility::levelOfOccurrence(core::Ptr<fusion::Occurrence> occurrence)
{
	if (occurrence == nullptr)
		return INT_MAX;

	std::string pathName = occurrence->fullPathName();

	int count = 0;
	for (char c : pathName)
		if (c == '+')
			count++;

	return count;
}

adsk::core::Ptr<adsk::fusion::Occurrence> Utility::lowerOccurrence(adsk::core::Ptr<adsk::fusion::Joint> joint)
{
	if (levelOfOccurrence(joint->occurrenceOne()) >= levelOfOccurrence(joint->occurrenceTwo()))
		return joint->occurrenceOne();
	else
		return joint->occurrenceTwo();
}

adsk::core::Ptr<adsk::fusion::Occurrence> Utility::upperOccurrence(adsk::core::Ptr<adsk::fusion::Joint> joint)
{
	if (levelOfOccurrence(joint->occurrenceOne()) < levelOfOccurrence(joint->occurrenceTwo()))
		return joint->occurrenceOne();
	else
		return joint->occurrenceTwo();
}
