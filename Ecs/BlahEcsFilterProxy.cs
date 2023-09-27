namespace Blah.Ecs
{
public class BlahEcsFilterProxy
{
	private BlahEcsFilter _filter;
    
	
	public void Finalize(BlahEcsFilter filter) => _filter = filter;

	public BlahEcsFilter.Enumerator GetEnumerator()
	{
		return _filter.GetEnumerator();
	}


	public bool IsSame(BlahEcsFilterProxy filter) => _filter == filter._filter;
}

    
public class BlahEcsFilter<T0> : BlahEcsFilterProxy
{
	//public class Exc<TE0> : BlahEcsFilter { }
}

public class BlahEcsFilter<T0, T1> : BlahEcsFilterProxy
{
	//public class Exc<TE0> : BlahEcsFilter { }
}

public class BlahEcsFilter<T0, T1, T2> : BlahEcsFilterProxy
{
	//public class Exc<TE0> : BlahEcsFilter { }
}
}