namespace ServiceLocator
{
	using System;

	interface IStringFileParseServiceList<KeyType, T>
	{
		T Select(KeyType key);
		System.Collections.Generic.Dictionary<KeyType, T> Services { get; }
	}
}
