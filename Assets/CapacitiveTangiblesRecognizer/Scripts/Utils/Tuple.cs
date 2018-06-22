
public struct Tuple<T, K> {
	public T first;
	public K second;

	public override string ToString ()
	{
		return string.Format ("({0}, {1})", first, second);
	}
}
