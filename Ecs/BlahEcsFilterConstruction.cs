namespace Blah.Ecs
{
public class BlahEcsFilter<TI0> : BlahEcsFilter
{
	public class Exc<TE0> : BlahEcsFilter { }
	public class Exc<TE0, TE1> : BlahEcsFilter { }
}

public class BlahEcsFilter<T0, T1> : BlahEcsFilter
{
	public class Exc<TE0> : BlahEcsFilter { }
	public class Exc<TE0, TE1> : BlahEcsFilter { }
}

public class BlahEcsFilter<T0, T1, T2> : BlahEcsFilter
{
	public class Exc<TE0> : BlahEcsFilter { }
	public class Exc<TE0, TE1> : BlahEcsFilter { }
}

public class BlahEcsFilter<T0, T1, T2, T3> : BlahEcsFilter
{
	public class Exc<TE0> : BlahEcsFilter { }
	public class Exc<TE0, TE1> : BlahEcsFilter { }
}
}