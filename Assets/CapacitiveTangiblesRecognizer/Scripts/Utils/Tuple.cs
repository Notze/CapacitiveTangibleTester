
public struct Tuple<T, K> {
	public T first;
	public K second;

	public Tuple (T first, K second){
		this.first = first;
		this.second = second;
	}

	public override string ToString ()
	{
		return string.Format ("({0}, {1})", first, second);
	}
}
