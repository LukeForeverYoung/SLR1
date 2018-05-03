using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SLR1.Grammer.GrammerItem;
using static SLR1.Sheet;

namespace SLR1
{
	class Program
	{
		static void Main(string[] args)
		{
			
			
			Grammer grammer = new Grammer(@"Grammer/GrammerTest.txt",@"Grammer/ClassCode.txt");
			Sheet sheet = new Sheet(grammer);
			Driver driver = new Driver(sheet);
			driver.Run(@"Token/tokens.txt");
			Console.Read();
			
		}
	}
	class Sheet
	{
		public Grammer grammer;
		List<ProjectSet> MainSet;
		Dictionary<int, Dictionary<int, Action>> actionSheet;
		public Sheet(Grammer grammer)
		{
			this.grammer = grammer;
			MainSet = new List<ProjectSet>();
			actionSheet = new Dictionary<int, Dictionary<int, Action>>();
			MakeRule();
			PrintSheet();
		}
		private ProjectSet FindSetByObject(ProjectSet item){
			foreach(var i in MainSet)
			{
				if (i.Equals(item)) return i;
			}
			return null;
		}
		public ProjectSet FindSetById(int id)
		{
			return MainSet[id];
		}
		public interface Action { }
		public class Shift : Action
		{
			public int nextState;
			public Shift(int nextState)
			{
				this.nextState = nextState;
			}
		}
		public class Reduce : Action
		{
			public EdgeItem reduceEdge;
			public Reduce(EdgeItem reduceEdge)
			{
				this.reduceEdge = reduceEdge;
			}
		}
		public class Accept:Action
		{
			//Nothing
		}
		public Action GetAction(int stateId,int symbolCode)
		{
			var stateSheet = actionSheet[stateId];
			if (stateSheet.ContainsKey(symbolCode))
				return stateSheet[symbolCode];
			return null;
		}
		
		public int[] GetStateAllFroms(int stateId)
		{
			List<int> list = new List<int>();
			foreach(var i in MainSet[stateId].projects)
			{
				list.Add(i.GetFrom());
			}
			return list.ToArray();
		}
		public bool HasFunctionLeftBracket(int stateId)
		{
			foreach(var i in MainSet[stateId].projects)
			{
				if (i.GetFrom() == grammer.Symbol2Code["K"] && i.RightSymbol() == grammer.Symbol2Code["("])
					return true;
			}
			return false;
		}
		private void AddSheetItem(int stateId,int nextSymbol,Action action)
		{
			Dictionary<int, Action> stateObject;
			if (actionSheet.ContainsKey(stateId))
			{
				stateObject = actionSheet[stateId];
			}
			else
			{
				stateObject = new Dictionary<int, Action>();
				actionSheet[stateId] = stateObject;
			}
			if(stateObject.ContainsKey(nextSymbol))
			{
				Console.WriteLine("Error: 构造项目转移时出错,存在冲突. " + stateId + " " + nextSymbol);
				return;
			}
			stateObject[nextSymbol] = action;
		}
		public class Project
		{
			EdgeItem edge;
			int pointPosition;
			public Project(EdgeItem edge)
			{
				this.edge = edge;
				pointPosition = 0;
			}
			public Project(EdgeItem edge,int position)
			{
				this.edge = edge;
				pointPosition = position;
			}
			public EdgeItem GetEdge()
			{
				return edge;
			}
			public int GetFrom()
			{
				return edge.GetFrom();
			}
			public Project NextProject()
			{
				if (isReduce()) return null;
				Project nextProject = new Project(edge, pointPosition + 1);
				return nextProject;
			}
			public bool isReduce()
			{
				return pointPosition == edge.to.Length;
			}
			public int RightSymbol()
			{
				return edge.to[pointPosition];
			}
			public override bool Equals(object obj)
			{
				if (obj == null) return false;
				if (obj.GetType() != GetType()) return false;
				var aobj = obj as Project;
				if (edge!=aobj.edge) return false;
				if(pointPosition!=aobj.pointPosition)return false;
				return true;
			}
			public override int GetHashCode()
			{
				return edge.GetHashCode()^pointPosition.GetHashCode();
			}
		}
		public class ProjectSet
		{
			static int idCount=0;
			static public void SubCount()
			{
				idCount--;
			}
			public int id;
			public HashSet<Project> projects;
			public ProjectSet()
			{
				id = idCount++;
				projects = new HashSet<Project>();
			}
			public void Add(Project item)
			{
				projects.Add(item);
			}
			public bool Contains(Project item)
			{
				return projects.Contains(item);
			}
			public override bool Equals(object obj)
			{
				if (obj == null) return false;
				if (obj.GetType() != GetType()) return false;
				var aobj = obj as ProjectSet;
				if (projects.Count() != aobj.projects.Count()) return false;
				foreach(var item in projects)
				{
					if (!aobj.projects.Contains(item)) return false;
				}
				return true;
			}
			public override int GetHashCode()
			{
				var v = 0;
				foreach(var item in projects)
				{
					v ^= item.GetHashCode();
				}
				return v;
			}
		}
		void Closure(ProjectSet set)
		{
			Queue<Project> q = new Queue<Project>();
			foreach(var item in set.projects)
			{
				q.Enqueue(item);
			}
			while(q.Count()!=0)
			{
				Project item = q.Dequeue();
				if (!item.isReduce()&&item.RightSymbol() < 0)
				{
					var edges = grammer.GetEdgesOf(item.RightSymbol());
					foreach(var edge in edges)
					{
						Project newProject = new Project(edge);
						if (set.Contains(newProject)) continue;
						set.Add(newProject);
						q.Enqueue(newProject);
					}
				}
			}
		}
		private void MakeRule()
		{
			ProjectSet initSet = new ProjectSet();
			foreach (var edge in grammer.GetEdgesOf(-1))
			{
				initSet.Add(new Project(edge));
			}
			Closure(initSet);
			MainSet.Add(initSet);
			//构造一个队列做BFS，MainSet用来做簇的去重,next指向下一个簇的引用
			Queue<ProjectSet> q = new Queue<ProjectSet>();
			q.Enqueue(initSet);
			while(q.Count()!=0)
			{
				ProjectSet nowState = q.Dequeue();
				Dictionary<int, List<Project> > classification = new Dictionary<int, List<Project> >();
				foreach (var project in nowState.projects)
				{
					if(project.isReduce())
					{
						foreach(var followItem in grammer.GetFollow(project.GetFrom()))
						{
							if (project.GetFrom()==-1)
								AddSheetItem(nowState.id, followItem, new Accept());
							else
								AddSheetItem(nowState.id, followItem, new Reduce(project.GetEdge()));
						}
					}
					else
					{
						if (!classification.ContainsKey(project.RightSymbol()))
							classification[project.RightSymbol()] = new List<Project>();
						classification[project.RightSymbol()].Add(project.NextProject());
					}
				}
				foreach (var item in classification)
				{
					var newSet = new ProjectSet();
					foreach(var project in item.Value)
					{
						newSet.Add(project);
					}
					Closure(newSet);
					var findItem = FindSetByObject(newSet);
					if (findItem!=null)
					{
						newSet = findItem;
						ProjectSet.SubCount();
					}
					else
					{
						MainSet.Add(newSet);
						q.Enqueue(newSet);
					}
					AddSheetItem(nowState.id, item.Key, new Shift(newSet.id));
				}
			}
		}
		
		public void PrintSheet()
		{
			List<ProjectSet> list = new List<ProjectSet>();
			foreach(var item in MainSet)
			{
				list.Add(item);
			}
			list.Sort((a, b) =>
			{
				return a.id - b.id;
			});
			foreach (var item in list)
			{
				Console.WriteLine("State "+item.id+":");
				foreach(var i in actionSheet[item.id])
				{
					Console.Write("nextSymbol: " + grammer.Code2Symbol[i.Key]+" ");
					if (i.Value is Shift)
						Console.WriteLine((i.Value as Shift).nextState);
					else if(i.Value is Reduce)
					{
						Console.WriteLine(grammer.Code2Symbol[(i.Value as Reduce).reduceEdge.GetFrom()] + " " + (i.Value as Reduce).reduceEdge.to.Length);
					}
				}
			}
			Console.WriteLine(MainSet.Count);
		}
		
	}
	class Driver
	{
		Stack<int> input;
		Stack<int> states;
		Sheet sheet;

		public Driver(Sheet sheet)
		{
			this.sheet = sheet;
		}
		private void Init(String tokenFilePath)
		{
			states = new Stack<int>();
			input = new Stack<int>();
			var tokens = ReadToken(tokenFilePath);
			for(int i=tokens.Length-1;i>=0;i--)
			{
				input.Push(tokens[i].code);
			}
			Console.WriteLine(input.Peek());

		}
		public void PrintAction(Sheet.Action action)
		{
			if (action == null) return;
			Console.WriteLine(action.GetType()+":");
			if(action is Shift)
			{
				Console.WriteLine(sheet.grammer.Code2Symbol[(action as Shift).nextState]);
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				var red = action as Reduce;
				stringBuilder.Append(sheet.grammer.Code2Symbol[red.reduceEdge.GetFrom()]+"->");
				foreach (var code in red.reduceEdge.to)
					stringBuilder.Append(sheet.grammer.Code2Symbol[code] + " ");
				Console.WriteLine(stringBuilder.ToString());
			}
		}
		private int GetBrackedIndex(int symbolCode)
		{
			int bracketIndex = -1;
			if (symbolCode == 31)
				bracketIndex = symbolCode - 31;
			if (bracketIndex == 32)
				bracketIndex = symbolCode - 32 + 3;
			if (symbolCode == 29)
				bracketIndex = symbolCode - 29 + 1;
			if (symbolCode == 30)
				bracketIndex = symbolCode - 30 + 4;
			if (symbolCode == 33)
				bracketIndex = symbolCode - 33 + 2;
			if (symbolCode == 34)
				bracketIndex = symbolCode - 34 + 3;
			return bracketIndex;
		}
		public void Run(String tokenFilePath)
		{
			int[] bracketStack = new int[3];
			Init(tokenFilePath);
			states.Push(0);
			bool flag = false;
			bool meetError = false;
			int preSymbol=-1;
			while(input.Count()!=0)
			{
				int nowState = states.Peek();
				int nextSymbol = input.Peek();
				var nowAction = sheet.GetAction(nowState, nextSymbol);
				//debug
				/*
				Console.WriteLine(nowState);
				Console.WriteLine(sheet.grammer.Code2Symbol[nextSymbol]);
				if(nowAction is Reduce)
				{
					StringBuilder stringBuilder = new StringBuilder();
					var red = nowAction as Reduce;
					stringBuilder.Append(sheet.grammer.Code2Symbol[red.reduceEdge.GetFrom()] + "->");
					foreach (var code in red.reduceEdge.to)
						stringBuilder.Append(sheet.grammer.Code2Symbol[code] + " ");
					Console.WriteLine(stringBuilder.ToString());
				}
				*/
				//
				//PrintAction(nowAction);
				if (nowAction is Accept)
				{
					flag = true;
					break;
				}
				else if(nowAction is Shift)
				{
					int brackedIndex = GetBrackedIndex(nextSymbol);
					if(brackedIndex!=-1)
					{
						if (brackedIndex < 3)
							bracketStack[brackedIndex]++;
						else
							bracketStack[brackedIndex-3]--;
					}
					preSymbol = nextSymbol;
					input.Pop();
					states.Push((nowAction as Shift).nextState);
				}
				else if(nowAction is Reduce)
				{
					var edge = (nowAction as Reduce).reduceEdge;
					for (int i = 0; i < edge.to.Length; i++)
						states.Pop();
					input.Push(edge.GetFrom());
				}
				else
				{
					meetError = true;
					if(	preSymbol== sheet.grammer.Symbol2Code["+"]||
						preSymbol== sheet.grammer.Symbol2Code["*"])
					{
						input.Push(sheet.grammer.Symbol2Code["number"]);
						Console.WriteLine("Expect a number after " + sheet.grammer.Code2Symbol[preSymbol]);
					}
					else if(nextSymbol == sheet.grammer.Symbol2Code["+"] ||
							nextSymbol == sheet.grammer.Symbol2Code["*"])
					{
						input.Push(sheet.grammer.Symbol2Code["number"]);
						Console.WriteLine("Expect a number after " + sheet.grammer.Code2Symbol[nextSymbol]);
					}
					else if (preSymbol == sheet.grammer.Symbol2Code["while"]|| preSymbol == sheet.grammer.Symbol2Code["for"])
					{
						input.Push(sheet.grammer.Symbol2Code["("]);
						bracketStack[0]++;
						Console.WriteLine("Expect '(' after "+ sheet.grammer.Code2Symbol[preSymbol]);
						continue;
					}
					else if (sheet.HasFunctionLeftBracket(nowState))
					{
						input.Push(sheet.grammer.Symbol2Code["("]);
						bracketStack[0]++;
						Console.WriteLine("Expect '(' after " + sheet.grammer.Code2Symbol[preSymbol]);
						continue;
					}
					else if (sheet.GetAction(nowState, sheet.grammer.Symbol2Code[")"])!=null&&bracketStack[0]!=0)
					{
						input.Push(sheet.grammer.Symbol2Code[")"]);
						Console.WriteLine("Expect ')' before " + sheet.grammer.Code2Symbol[nextSymbol]);
						continue;
					}
					else if (sheet.GetAction(nowState, sheet.grammer.Symbol2Code["]"]) != null && bracketStack[1] != 0)
					{
						input.Push(sheet.grammer.Symbol2Code["]"]);
						Console.WriteLine("Expect ')' before " + sheet.grammer.Code2Symbol[nextSymbol]);
						continue;
					}
					else if (sheet.GetAction(nowState, sheet.grammer.Symbol2Code["}"]) != null && bracketStack[2] != 0)
					{
						input.Push(sheet.grammer.Symbol2Code["}"]);
						Console.WriteLine("Expect ')' before " + sheet.grammer.Code2Symbol[nextSymbol]);
						continue;
					}
					else
					{
						Console.WriteLine("UnExpected symbol "+ sheet.grammer.Code2Symbol[nextSymbol]);
						input.Pop();
						Console.WriteLine(input.Count());
						continue;
					}
				}
			}
			if (flag&&!meetError)
				Console.WriteLine("Accepted");
			else
			{
				Console.WriteLine("Failed");
			}
		}
		class Token
		{
			public String content { get; set; }
			public int code { get; set; }
			public int line { get; set; }
			public Token(String content, int code, int line)
			{
				this.content = content;
				this.code = code;
				this.line = line;
			}
		}
		Token[] ReadToken(String filePath)
		{
			List<Token> list = new List<Token>();
			String str;
			try
			{
				FileStream fs = new FileStream(filePath, FileMode.Open);
				StreamReader rs = new StreamReader(fs);
				int line = 0;
				while (!rs.EndOfStream)
				{
					str = rs.ReadLine();
					if (str.Split(' ')[0] != @"\n")
						list.Add(new Token(str.Split(' ')[0], Int32.Parse(str.Split(' ')[1]), line));
					else
						line++;
				}
				rs.Close();
				fs.Close();
			}
			catch (Exception e) { throw e; }
			return list.ToArray();
		}
	}
	/**
		 * Grammar List
		 * EOF	:	38
		 * #	:	
		 * NA	:	0  void
		 * B	:	11 word
		 * N	:	12 number
		 * +	:	13
		 * -	:	14
		 * *	:	15
		 * /	:	16
		 * (	:	31
		 * )	:	32
		 * S -> E | NA 
		 * E -> NE' | BE' | (E)E'
		 * E'-> FEE' | NA
		 * F-> + | - | * | /
		 * */
	
	class Grammer
	{
		const int Void = 0;
		const int EOF = 38;
		public class GrammerItem
		{
			int from;
			public int GetFrom()
			{
				return from;
			}
			public class EdgeItem
			{
				int from;
				public int[] to;
				public HashSet<int> selectSet;
				public EdgeItem(int[] to,int from)
				{
					this.from = from;
					this.to = to;
					selectSet = new HashSet<int>();
				}
				public int GetFrom()
				{
					return from;
				}
			}
			public List<EdgeItem> edges { get; private set; }
			public HashSet<int> firstSet;
			public HashSet<int> followSet;
			public GrammerItem(int from)
			{
				this.from = from;
				edges = new List<EdgeItem>();
				followSet = new HashSet<int>();
			}
			public void AddTo(int[] to)
			{
				edges.Add(new EdgeItem(to,from));
			}
		}
		HashSet<int> GetFirst(int[] items)
		{
			HashSet<int> firsts = new HashSet<int>();
			bool voidFlag = true;
			foreach (var it in items)
			{
				if (it >= 0)
				{
					firsts.Add(it);
					if (it > 0)
					{
						voidFlag = false;
						break;
					}
				}
				else
				{
					var itsSet = MakeFirst(grammerMap[it]);
					firsts.UnionWith(itsSet);
					if (!itsSet.Contains(Void))
					{
						voidFlag = false; 
						break;
					}
				}
			}
			firsts.Remove(Void);
			if (voidFlag)
				firsts.Add(Void);
			return firsts;
		}
		HashSet<int> MakeFirst(GrammerItem now)
		{
			if (now.firstSet != null)
			{
				//Console.WriteLine("First递归"+Code2Symbol[now.GetFrom()]);
				return now.firstSet;
			}
			now.firstSet = new HashSet<int>();
			foreach (var edge in now.edges)
			{
				now.firstSet.UnionWith(GetFirst(edge.to));
			}
			return now.firstSet;
		}
		void MakeFirst()
		{
			foreach(var pair in grammerMap)
			{
				MakeFirst(pair.Value);
			}
		}
		
		void MakeFollow()
		{
			Queue<int> queue = new Queue<int>();
			grammerMap[-1].followSet.Add(EOF);
			queue.Enqueue(-1);
			foreach(var item in grammerMap)
			{
				foreach(var edge in item.Value.edges)
				{
					for(int i=0;i<edge.to.Length;i++)
					{
						if(edge.to[i]<0)
						{
							var symbol = grammerMap[edge.to[i]];
							int size = symbol.followSet.Count();
							var firstWithoutVoid = GetFirst(edge.to.Skip(i + 1).ToArray());
							firstWithoutVoid.Remove(Void);
							symbol.followSet.UnionWith(firstWithoutVoid);
							if (symbol.followSet.Count() > size) queue.Enqueue(edge.to[i]);
						}
					}
				}
			}
			while(queue.Count()!=0)
			{
				var item= grammerMap[queue.Dequeue()];
				foreach(var edge in item.edges)
				{
					for(int i=0;i<edge.to.Length;i++)
					{
						if(edge.to[i]<0)
						{
							var symbol = grammerMap[edge.to[i]];
							int size = symbol.followSet.Count();
							if(GetFirst(edge.to.Skip(i + 1).ToArray()).Contains(Void))
							{
								symbol.followSet.UnionWith(item.followSet);
								if (symbol.followSet.Count() > size)
									queue.Enqueue(edge.to[i]);
							}
						}
					}
				}
			}
		}
		void MakeSelect()
		{
			foreach(var entry in grammerMap)
			{
				foreach(var item in entry.Value.edges)
				{
					var firstWithoutVoid = GetFirst(item.to);
					firstWithoutVoid.Remove(Void);
					item.selectSet.UnionWith(firstWithoutVoid.Union(entry.Value.followSet));
				}
			}
		}
		Dictionary<int, GrammerItem> grammerMap;
		/*
		 * Defination：Non_TerminalSymbol index begin at -1,
		 * TerminalSymbol index depend on symbol code sheet.
		 * */
		public Dictionary<String, int> Symbol2Code;
		public Dictionary<int, String> Code2Symbol;
		int Non_TerminalSymbolCount;
		private bool isNon_TerminalSymbol(String s)
		{
			if ('A' <= s[0] && s[0] <= 'Z') return true;
			return false;
		}
		private int GetSymbolCode(String s)
		{
			if (Symbol2Code.ContainsKey(s))
				return Symbol2Code[s];
			else
			{
				if(isNon_TerminalSymbol(s))
				{
					int id = Non_TerminalSymbolCount--;
					Symbol2Code.Add(s, id);
					Code2Symbol.Add(id, s);
					return id;
				}
				else
				{
					Console.WriteLine("Error: 终结符必须在读入符号表时全部导入,在产生式中发现了不合法的终结符.");
					return 0;
				}
			}
		}
		private void ReadGrammer(String filePath,String classCodePath)
		{
			Non_TerminalSymbolCount = -1;
			Symbol2Code = new Dictionary<string, int>();
			Code2Symbol = new Dictionary<int, string>();
			try
			{
				FileStream fs = new FileStream(classCodePath, FileMode.Open);
				StreamReader rs = new StreamReader(fs);
				while (!rs.EndOfStream)
				{
					String[] line = rs.ReadLine().Split(' ');
					int code = Int32.Parse(line[1]);
					Symbol2Code.Add(line[0], code);
					Code2Symbol.Add(code, line[0]);
				}
				rs.Close();
				fs.Close();
			}
			catch (Exception e) { throw e; }
			//读文法并转化
			grammerMap = new Dictionary<int, GrammerItem>();
			try
			{
				FileStream fs = new FileStream(filePath, FileMode.Open);
				StreamReader rs = new StreamReader(fs);
				while (!rs.EndOfStream)
				{
					String[] line = rs.ReadLine().Split(' ');
					int from=0;
					List<int> to = new List<int>();
					for(int i=0;i<line.Length;i++)
					{
						if (i == 0)
						{
							from = GetSymbolCode(line[i]);
						}
						else
						{
							to.Add(GetSymbolCode(line[i]));
						}
					}
					if (!grammerMap.ContainsKey(from))
						grammerMap[from] = new GrammerItem(from);
					grammerMap[from].AddTo(to.ToArray());
				}
				rs.Close();
				fs.Close();
			}
			catch (Exception e) { throw e; }
		}
		public EdgeItem[] GetEdgesOf(int v)
		{
			return grammerMap[v].edges.ToArray();
		}
		public void PrintFirst()
		{
			foreach (var entry in grammerMap)
			{
				Console.WriteLine(entry.Key);
				foreach (var f in entry.Value.firstSet)
				{
					Console.Write(f + "\t");
				}
				Console.WriteLine();
			}
		}
		public int[] GetFollow(int v)
		{
			return grammerMap[v].followSet.ToArray();
		}
		public void PrintFollow()
		{
			foreach (var entry in grammerMap)
			{
				Console.WriteLine(Code2Symbol[entry.Key]);
				foreach (var f in entry.Value.followSet)
				{
					Console.Write(Code2Symbol[f] + "\t");
				}
				Console.WriteLine();
			}
		}
		private void RemoveVoid()
		{
			foreach(var item in grammerMap)
			{
				foreach(var edge in item.Value.edges)
				{
					if (edge.to.Contains(Void))
						edge.to = edge.to.Take(0).ToArray();
				}
			}
		}
		public Grammer(String filePath,String classCodePath)
		{
			ReadGrammer(filePath,classCodePath);
			//SLR1部分不直接求first,因为只需要follow集,为了预防不必要的左递归.
			//MakeFirst();
			MakeFollow();

			//PrintFollow();
			//MakeSelect();
			//删除空产生式后就不能求各种集了,为项目集做准备
			var a=GetFirst(new int[] { Symbol2Code["D"] });
			foreach(var i in a)
				Console.Write(Code2Symbol[i] + " ");
			//Console.Read();
			RemoveVoid();
		}
	}
}
