using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blah.Ecs
{
public class BlahEcsInjectSource
{
	private readonly BlahEcs _ecs;

	private readonly List<Type>    _tempIncList = new();
	private readonly List<Type>    _tempExcList = new();
	
	
	public BlahEcsInjectSource(BlahEcs ecs)
	{
		_ecs = ecs;
	}

	public BlahEcs GetEcs() => _ecs;

	public T GetFilter<T>() where T : BlahEcsFilter, new()
	{
		/*
		_tempIncList.Clear();
		_tempExcList.Clear();

		var filter = new T();
		if (filter.Required != null)
			_tempIncList.AddRange(filter.Required);
		if (filter.Exc != null)
			_tempExcList.AddRange(filter.Exc);

		var fields = typeof(T).GetFields(
			BindingFlags.Instance | BindingFlags.Public
		);
		foreach (var field in fields)
			if (field.FieldType.IsGenericType &&
			    field.FieldType.GetGenericTypeDefinition() == typeof(IBlahEcsCompRead<>))
			{
				var arg = field.FieldType.GenericTypeArguments[0];
				_tempIncList.Add(arg);

				field.SetValue(filter, _ecs.GetPool(arg));
			}

		var core = _ecs.GetFilterCore(_tempIncList, _tempExcList);
		filter.SetCore(core);
		return filter;*/
		return null;
	}
}
}