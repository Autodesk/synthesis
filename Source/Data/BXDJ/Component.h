#pragma once

#include "XmlWriter.h"

namespace BXDJ
{
	class Driver;

	class Component : public XmlWritable
	{
	public:
		Component(Driver *);

	protected:
		Driver * driver;

	};
}
