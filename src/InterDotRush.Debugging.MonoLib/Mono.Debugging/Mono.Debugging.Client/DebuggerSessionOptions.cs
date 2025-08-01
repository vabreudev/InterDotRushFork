﻿// 
// DebuggerSessionOptions.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Mono.Debugging.Client
{
	[Serializable]
	public enum AutomaticSourceDownload
	{
		Ask,
		Always,
		Never
	}

	[Serializable]
	public class DebuggerSessionOptions
	{
		public EvaluationOptions EvaluationOptions { get; set; }
		public bool StepOverPropertiesAndOperators { get; set; }
		public bool ProjectAssembliesOnly { get; set; }
		public bool AutomaticSourceLinkDownload { get; set; }
		public bool DebugSubprocesses { get; set; }
		public ImmutableArray<string> SymbolSearchPaths { get; set; } = ImmutableArray<string>.Empty;
		public ImmutableDictionary<string, string> SourceCodeMappings { get; set; } = ImmutableDictionary<string, string>.Empty;
		public bool SearchMicrosoftSymbolServer { get; set; }
		public bool SearchNuGetSymbolServer { get; set; }
		public bool SkipNativeTransitions { get; set; }
	}
}
