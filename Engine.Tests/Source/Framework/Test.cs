﻿/* 
 * Class: GregValure.NaturalDocs.Engine.Tests.Framework.Test
 * ____________________________________________________________________________
 * 
 * A class storing information about a single file-based test.
 * 
 * Usage:
 * 
 *		- Iterate through the test data folder to create Test objects from files.
 *			- In most cases there will be a 1:1 ratio between tests and input files.  In this case you can test each
 *				file with <IsInputFile()> and create the object with <FromInputFile()>.
 *			- If a test requires multiple input files, you can test each file with <IsExpectedOutputFile()> and create
 *				objects with <FromExpectedOutputFile()>.  However, you may also want to use <IsActualOutputFile()>
 *				and <FromActualOutputFile()> if you want to detect actual output files that don't have corresponding 
 *				expected output files and count them as failures.
 *		- Generate the output for the test.
 *			- One option is to generate the output and call <SetActualOutput()>.  When the test runs it will store
 *				this in <ActualOutputFile> and compare it to the contents of <ExpectedOutputFile>.
 *			- Another option is to generate all the actual output files ahead of time.  Then when each test runs
 *				they will load the contents from <ActualOutputFile> and compare it to the contents of <ExpectedOutputFile>.
 *			- If an exception occurs when generating the output, store it in <TestException>.  This only applies if the
 *				exception is unexpected as it will cause the test to fail.  If it's an expected part of the test, it must be
 *				captured and added to the output instead.
 *		- Call <Run()> to execute the test.
 *		- Use <Passed> to see the status.
 * 
 */

// This file is part of Natural Docs, which is Copyright © 2003-2012 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using GregValure.NaturalDocs.Engine;


namespace GregValure.NaturalDocs.Engine.Tests.Framework
	{
	public class Test
		{

		// Group: Functions
		// __________________________________________________________________________

		/* Constructor: Test
		 */
		protected Test ()
			{
			name = null;

			inputFile = null;
			expectedOutputFile = null;
			actualOutputFile = null;

			actualOutput = null;
			testException = null;

			passed = false;
			testWasRun = false;
			}


		/* Function: FromInputFile
		 * Creates a new Test object from an input file.
		 */
		public static Test FromInputFile (Path inputFile)
			{
			Test test = new Test();
			Path testFolder = inputFile.ParentFolder;

			test.name = TestNameFromInputFile(inputFile);
			test.inputFile = inputFile;
			test.expectedOutputFile = ExpectedOutputFileOf(test.name, testFolder);
			test.actualOutputFile = ActualOutputFileOf(test.name, testFolder);

			return test;
			}


		/* Function: FromExpectedOutputFile
		 * Creates a new Test object from an expected output file.
		 */
		public static Test FromExpectedOutputFile (Path outputFile)
			{
			Test test = new Test();
			Path testFolder = outputFile.ParentFolder;

			test.name = TestNameFromExpectedOutputFile(outputFile);
			test.inputFile = null;
			test.expectedOutputFile = outputFile;
			test.actualOutputFile = ActualOutputFileOf(test.name, testFolder);

			return test;
			}


		/* Function: FromActualOutputFile
		 * Creates a new Test object from an actual output file.
		 */
		public static Test FromActualOutputFile (Path outputFile)
			{
			Test test = new Test();
			Path testFolder = outputFile.ParentFolder;

			test.name = TestNameFromActualOutputFile(outputFile);
			test.inputFile = null;
			test.expectedOutputFile = ExpectedOutputFileOf(test.name, testFolder);
			test.actualOutputFile = outputFile;

			return test;
			}


		/* Function: SetActualOutput
		 * Sets the actual output generated from the test.  This will replace the contents of <ActualOutputFile> when
		 * <Run()> is called.
		 */
		public void SetActualOutput (string output)
			{
			actualOutput = output;

			// DEPENDENCY: Run() requires actualOutput to not be null if this was ever called.
			// DEPENDENCY: Run() requires the string generated for null to be consistent with the one it generates.

			if (actualOutput == null)
				{  actualOutput = "(No output generated)\n";  }
			}


		/* Function: Run
		 * Executes the test.  If <SetActualOutput()> was called, this will compare it to <ExpectedOutputFile> and be saved
		 * to <ActualOutputFile>.  If <SetActualOutput()> was not called, <ActualOutputFile> will be loaded and compared to 
		 * <ExpectedOutputFile>.
		 */
		public void Run ()
			{
			if (testWasRun)
				{  return;  }


			// Load actual output

			// DEPENDENCY: This requires actualOutput to never be null if SetActualOutput() was called, even if it was passed 
			// a null string.
			if (actualOutput == null)
				{
				try
					{  actualOutput = System.IO.File.ReadAllText(actualOutputFile);  }
				catch (Exception e)
					{
					if (e is System.IO.FileNotFoundException || e is System.IO.DirectoryNotFoundException)
						{  throw new Exception("Could not find actual output file " + actualOutputFile + ".");  }
					else if (e is System.IO.IOException)
						{  throw new Exception ("Could not open actual output file " + actualOutputFile + ".", e);  }
					else
						{  throw;  }
					}
				}


			// Load expected output

			string expectedOutput;

			try
				{  expectedOutput = System.IO.File.ReadAllText(expectedOutputFile);  }
			catch (Exception e)
				{
				if (e is System.IO.FileNotFoundException || e is System.IO.DirectoryNotFoundException)
					{  throw new Exception("Could not find expected output file " + expectedOutputFile + ".");  }
				else if (e is System.IO.IOException)
					{  throw new Exception ("Could not open expected output file " + expectedOutputFile + ".", e);  }
				else
					{  throw;  }
				}


			// Fill in replacement values for exceptions and null
			
			if (testException != null)
				{
				actualOutput =
					"Exception thrown:\n\n" +
					"   " + testException.Message + "\n" +
					"   (" + testException.GetType() + ")\n\n";

				Exception inner = testException.InnerException;
				while (inner != null)
					{
					actualOutput +=
					"Caused by:\n\n" +
					"   " + inner.Message + "\n" +
					"   (" + inner.GetType() + ")\n\n";

					inner = inner.InnerException;
					}

				actualOutput +=
					"Stack trace:\n\n" +
					testException.StackTrace + "\n";
				}

			else if (actualOutput == null || actualOutput == "")
				{
				// DEPENDENCY: This must be consistent with what SetActualOutput() generates for null.
				actualOutput = "(No output generated)\n";
				}


			// Run the actual test

			passed = (expectedOutput.NormalizeLineBreaks() == actualOutput.NormalizeLineBreaks());
			testWasRun = true;


			// Save actual output if necessary

			string oldOutput = null;

			try
				{  
				// May not exist, don't throw exceptions if we can't read it.  If it was required to exist because SetActualOutput() 
				// was never called, exceptions for that would have been thrown at the beginning of the function.
				oldOutput = System.IO.File.ReadAllText(actualOutputFile);  
				}
			catch
				{  oldOutput = null;  }

			// We don't want to change the time stamps every time we run, so only write when necessary.
			if (oldOutput == null || oldOutput.NormalizeLineBreaks() != actualOutput.NormalizeLineBreaks())
				{  System.IO.File.WriteAllText(actualOutputFile, actualOutput);  }
			}



		// Group: Path Functions
		// __________________________________________________________________________


		/* Function: IsInputFile
		 *	 Returns whether the passed file is a test input file.
		 */
		public static bool IsInputFile (Path file)
			{
			// Input files can have any extension
			return file.NameWithoutPathOrExtension.EndsWith(" - Input");
			}
		
		/* Function: IsExpectedOutputFile
		 *	 Returns whether the passed file is an expected output file for a test.
		 */
		public static bool IsExpectedOutputFile (Path file)
			{
			return file.NameWithoutPath.EndsWith(" - Expected Output.txt");
			}

		/* Function: IsActualOutputFile
		 *	 Returns whether the passed file is an actual output file for a test.
		 */
		public static bool IsActualOutputFile (Path file)
			{
			return file.NameWithoutPath.EndsWith(" - Actual Output.txt");
			}

		/* Function: TestNameFromInputFile
		 * Extracts the name of the test from an input file path.
		 */
		public static string TestNameFromInputFile (Path inputFile)
			{
			string name = inputFile.NameWithoutPathOrExtension;

			if (name.EndsWith(" - Input") == false)
				{  throw new InvalidOperationException();  }
			else
				{  return name.Substring(0, name.Length - 8);  }
			}		

		/* Function: TestNameFromExpectedOutputFile
		 * Extracts the name of the test from an expected output file path.
		 */
		public static string TestNameFromExpectedOutputFile (Path outputFile)
			{
			string name = outputFile.NameWithoutPath;

			if (name.EndsWith(" - Expected Output.txt") == false)
				{  throw new InvalidOperationException();  }
			else
				{  return name.Substring(0, name.Length - 22);  }
			}
		
		/* Function: TestNameFromActualOutputFile
		 * Extracts the name of the test from an actual output file path.
		 */
		public static string TestNameFromActualOutputFile (Path outputFile)
			{
			string name = outputFile.NameWithoutPath;

			if (name.EndsWith(" - Actual Output.txt") == false)
				{  throw new InvalidOperationException();  }
			else
				{  return name.Substring(0, name.Length - 20);  }
			}

		/* Function: ExpectedOutputFileOf
		 * Returns the expected output file of the passed test name and folder.
		 */
		public static Path ExpectedOutputFileOf (string testName, Path testFolder)
			{
			return testFolder + '/' + testName + " - Expected Output.txt";
			}
		
		/* Function: ActualOutputFileOf
		 * Returns the actual output file of the passed test name and folder.
		 */
		public static Path ActualOutputFileOf (string testName, Path testFolder)
			{
			return testFolder + '/' + testName + " - Actual Output.txt";
			}



		// Group: Properties
		// _________________________________________________________________________


		/* Property: Passed
		 * Whether the test succeeded, which means the actual output matches the expected output and no exceptions were 
		 * thrown.  You cannot access this property before calling <Run()>.
		 */
		public bool Passed
			{
			get
				{
				if (testWasRun == false)
					{  throw new Exception("Cannot access Test.Passed before the test was run.");  }

				return passed;
				}
			}

		/* Property: Name
		 * The name of the test.
		 */
		public string Name
			{
			get
				{  return name;  }
			}

		/* Property: InputFile
		 * The full path of the input file.
		 */
		public Path InputFile
			{
			get
				{  return inputFile;  }
			}

		/* Property: ExpectedOutputFile
		 * The full path of the expected output file.
		 */
		public Path ExpectedOutputFile
			{
			get
				{  return expectedOutputFile;  }
			}

		/* Property: ActualOutputFile
		 * The full path of the actual output file where you put the generated output if it differs from <ExpectedOutput>.
		 */
		public Path ActualOutputFile
			{
			get
				{  return actualOutputFile;  }
			}

		/* Property: TestException
		 * The exception generated by trying to run the test, if any.  If this is set the test will fail.
		 */
		public Exception TestException
			{
			get
				{  return testException;  }
			set
				{  testException = value;  }
			}



		// Group: Variables
		// _________________________________________________________________________


		/* var: name
		 * The name of the test.
		 */
		protected string name;

		/* var: inputFile
		 * The full path of the input file.
		 */
		protected Path inputFile;

		/* var: expectedOutputFile
		 * The full path of the expected output file.
		 */
		protected Path expectedOutputFile;

		/* var: actualOutputFile
		 * The full path of the actual output file.
		 */
		protected Path actualOutputFile;

		/* var: actualOutput
		 * The actual output generated from the test if it is set directly from the property instead of being 
		 * loaded from <actualOutputFile>.  Use <actualOutputWasSet> to determine its status.
		 */
		protected string actualOutput;

		/* var: testException
		 * The exception generated by trying to build the output, if any.
		 */
		protected Exception testException;

		/* var: passed
		 * Whether the test has passed.  This value is only valid if <testWasRun> is true.
		 */
		protected bool passed;

		/* var: testWasRun
		 * WHether the test was executed.
		 */
		protected bool testWasRun;

		}

	}