using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CriticalColumnCell : MonoBehaviour {

	private Rigidbody rb;

	public float torqueMagnitude = 10.0f;

	[HideInInspector]
	public int matchReq;

	private Dictionary<Cell, List<Cell>> matchDict;

	[HideInInspector]
	public bool instantClear = false;

	private void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody>();

		rb.AddTorque(transform.forward * torqueMagnitude);

		matchDict = new Dictionary<Cell, List<Cell>>();
	}

	public void OnTriggerEnter(Collider other)
	{
		if(other.tag.Equals("Cell"))
		{
			Cell c = other.GetComponent<Cell>();

			if(c.ccCell != this)
			{
				c.ccCell = this;

				CheckForMatches (c);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.tag.Equals("Cell"))
		{
			Cell c = other.GetComponent<Cell>();

			if(c.ccCell == this)
			{
				c.ccCell = null;

				c.matched = false;

				if(matchDict.ContainsKey (c))
				{
					for(int i = 0; i < matchDict[c].Count; i++)
					{
						if(matchDict[c][i] != c)
						{
							matchDict[c][i].matched = false;

							if(matchDict[c][i].ccCell != null)
							{
								matchDict[c][i].ccCell.matchDict.Remove (matchDict[c][i]);

								matchDict[c][i].ccCell.CheckForMatches (matchDict[c][i]);
							}
						}
					}

					matchDict.Remove (c);
				}
			}
		}
	}

	private void CheckForMatches(Cell c)
	{
		Vector2 pos = new Vector2(c.transform.GetSiblingIndex(), c.transform.parent.GetSiblingIndex ());

		List<Cell> l = new List<Cell>();
		l.Add (c);

		int adjustedX = (int) (pos.x + Board.Instance.RowList[(int) pos.y].Offset);

		Matches (new Vector2(adjustedX, pos.y + 1), ref l);
		Matches (new Vector2(adjustedX, pos.y - 1), ref l);

		if(l.Count >= matchReq)
		{
			if(instantClear)
			{
				Board.Instance.Match ();
			}
			else
			{
				for(int i = 0; i < l.Count; i++)
				{
					l[i].matched = true;

					if(l[i].ccCell.matchDict.ContainsKey (l[i]))
					{
						if(l.Count > l[i].ccCell.matchDict[l[i]].Count)
						{
							l[i].ccCell.matchDict[l[i]] = l;
						}
					}
					else
					{
						l[i].ccCell.matchDict.Add (l[i], l);
					}
				}
			}
		}
	}

	private void Matches(Vector2 coord, ref List<Cell> matchList)
	{
		if(coord.y >= 0 && coord.y < Board.Instance.RowList.Count)
		{
			RowData rd = Board.Instance.RowList[(int)coord.y];

			if(coord.x + rd.Offset < rd.CellCount)
			{
				Cell c = rd.Cells[(int)coord.x + rd.Offset];

				if(c.ccCell != null && c.Match(matchList[0]) && !matchList.Contains (c))
				{
					matchList.Add (c);

					Matches (new Vector2(coord.x, coord.y + 1), ref matchList);
					Matches (new Vector2(coord.x, coord.y - 1), ref matchList);
				}
			}
		}
	}
}
