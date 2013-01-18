﻿/* 
 * Class: GregValure.NaturalDocs.Engine.Languages.ParentElement
 * ____________________________________________________________________________
 * 
 * A <Element> that also may contain child elements.  This is used not only for classes but for groups (so that settings
 * may be inherited) and list topics and enums (to store embedded topics.)
 * 
 */

// This file is part of Natural Docs, which is Copyright © 2003-2012 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using System.Collections.Generic;
using GregValure.NaturalDocs.Engine.Symbols;
using GregValure.NaturalDocs.Engine.Tokenization;


namespace GregValure.NaturalDocs.Engine.Languages
	{

	public class ParentElement : Element
		{

		// Group: Functions
		// __________________________________________________________________________


		/* Constructor: ParentElement
		 */
		public ParentElement (TokenIterator iterator, Flags flags) : base (iterator, flags)
			{  
			CommonInit();
			}


		/* Constructor: ParentElement
		 */
		public ParentElement (LineIterator lineIterator, Flags flags) : base (lineIterator, flags)
			{  
			CommonInit();
			}


		/* Constructor: ParentElement
		 */
		public ParentElement (int lineNumber, int charNumber, Flags flags) : base (lineNumber, charNumber, flags)
			{  
			CommonInit();
			}


		/* Function: CommonInit
		 * Common initialization done from all the constructors.
		 */
		private void CommonInit ()
			{
			isRootElement = false;

			maximumEffectiveChildAccessLevel = AccessLevel.Unknown;
			defaultDeclaredChildAccessLevel = AccessLevel.Unknown;
			defaultChildLanguageID = 0;
			defaultChildClassString = new ClassString();
			defaultChildClassStringSet = false;
			childContextString = new ContextString();
			childContextStringSet = false;

			endingLineNumber = -1;
			endingCharNumber = -1;
			}


		/* Function: Contains
		 * Whether the passed <Element> falls within the influence of this ParentElement.
		 */
		public bool Contains (Element element)
			{
			#if DEBUG
			if (endingLineNumber == -1 || endingCharNumber == -1)
				{  throw new Exception("Can't use ParentElement.Contains() if the ending line and char numbers weren't set.");  }
			#endif

			if (element.LineNumber > this.LineNumber && element.LineNumber < this.EndingLineNumber)
				{  return true;  }
			if (element.LineNumber < this.LineNumber || element.LineNumber > this.EndingLineNumber)
				{  return false;  }

			// If we're here it means the element's line number matches the starting or ending line number.

			if (element.LineNumber == this.LineNumber)
				{
				if (element.CharNumber < this.CharNumber)
					{  return false;  }
				if (this.EndingLineNumber == this.LineNumber && element.CharNumber >= this.EndingCharNumber)
					{  return false;  }

				return true;
				}
			else // element.LineNumber == this.EndingLineNumber
				{
				if (element.CharNumber >= this.EndingCharNumber)
					{  return false;  }
				if (this.LineNumber == this.EndingLineNumber && element.CharNumber < this.CharNumber)
					{  return false;  }

				return true;
				}
			}



		// Group: Properties
		// __________________________________________________________________________


		/* Property: IsRootElement
		 * Whether this element is intended to be the root for the file.
		 */
		public bool IsRootElement
			{
			get
				{  return isRootElement;  }
			set
				{  isRootElement = value;  }
			}

		/* Property: MaximumEffectiveChildAccessLevel
		 * The maximum effective access level children can attain, which is usually the effective access level of the parent.  For
		 * example, a private class's children wolud have a maximum effective access level of private, even if they were declared
		 * public.  Will be <AccessLevel.Unknown> if it's not set.
		 */
		public AccessLevel MaximumEffectiveChildAccessLevel
			{
			get
				{  return maximumEffectiveChildAccessLevel;  }
			set
				{  maximumEffectiveChildAccessLevel = value;  }
			}

		/* Property: DefaultDeclaredChildAccessLevel
		 * The default declared access level of this element's children if not otherwise specified.  This can be greater than
		 * <MaximumEffectiveChildAccessLevel> because it's only the declared access level.  Will be <AccessLevel.Unknown> if
		 * it's not set.
		 */
		public AccessLevel DefaultDeclaredChildAccessLevel
			{
			get
				{  return defaultDeclaredChildAccessLevel;  }
			set
				{  defaultDeclaredChildAccessLevel = value;  }
			}

		/* Property: DefaultChildLanguageID
		 * The language ID that applies to all children, or zero if it's not set.
		 */
		public int DefaultChildLanguageID
			{
			get
				{  return defaultChildLanguageID;  }
			set
				{  defaultChildLanguageID = value;  }
			}

		/* Property: DefaultChildClassString
		 * The default <ClassString> all children should use, if set.  Use <DefaultChildClassStringSet> to see if this value is 
		 * relevant, as null can be a valid value.  Setting this value will automatically set <DefaultChildClassStringSet> to true.
		 */
		public ClassString DefaultChildClassString
			{
			get
				{  return defaultChildClassString;  }
			set
				{  
				defaultChildClassString = value;  
				defaultChildClassStringSet = true;
				}
			}

		/* Property: DefaultChildClassStringSet
		 * Whether <DeaultChildClassString> was set for this element, and thus whether its value is relevant.
		 */
		public bool DefaultChildClassStringSet
			{
			get
				{  return defaultChildClassStringSet;  }
			set
				{  defaultChildClassStringSet = value;  }
			}

		/* Property: ChildContextString
		 * The <ContextString> all children should use, if set.  Use <ChildContextStringSet> to see if this value is relevant, as
		 * null can be a valid value.  Setting this value will automatically set <ChildContextStringSet> to true.
		 */
		public ContextString ChildContextString
			{
			get
				{  return childContextString;  }
			set
				{  
				childContextString = value;  
				childContextStringSet = true;
				}
			}

		/* Property: ChildContextStringSet
		 * Whether <ChildContextString> was set for this element, and thus whether its value is relevant.
		 */
		public bool ChildContextStringSet
			{
			get
				{  return childContextStringSet;  }
			set
				{  childContextStringSet = value;  }
			}

		/* Property: EndingLineNumber
		 * The line number where the parent's influence ends, or -1 if it hasn't been set yet.  The first line number is one, 
		 * not zero.
		 */
		public int EndingLineNumber
			{
			get
				{  return endingLineNumber;  }
			set
				{  endingLineNumber = value;  }
			}

		/* Property: EndingCharNumber
		 * The character number where the parent's influence ends, or -1 if it hasn't been set yet.  The first character number 
		 * is one, not zero, and is relative to the line, not the file.
		 */
		public int EndingCharNumber
			{
			get
				{  return endingCharNumber;  }
			set
				{  endingCharNumber = value;  }
			}



		// Group: Variables
		// __________________________________________________________________________


		/* var: isRootElement
		 * Whether this element is intended to be the file's root element.
		 */
		protected bool isRootElement;

		/* var: maximumEffectiveChildAccessLevel
		 * The maximum effective access level children can attain, which is usually the effective access level of the parent.  For 
		 * example, a private class's children would have a maximum effective access level of private, even if they were declared 
		 * public.  Will be <AccessLevel.Unknown> if it's not set.
		 */
		protected AccessLevel maximumEffectiveChildAccessLevel;

		/* var: defaultDeclaredChildAccessLevel
		 * The default declared access level of children.  This can be greater than <maximumEffectiveChildAccessLevel> because
		 * it is only the declared access level.  Will be <AccessLevel.Unknown> if it's not set.
		 */
		protected AccessLevel defaultDeclaredChildAccessLevel;

		/* var: defaultChildLanguageID
		 * The default language ID of all children, or zero if it's not set.
		 */
		protected int defaultChildLanguageID;

		/* var: defaultChildClassString
		 * The default <ClassString> to be used by children.
		 */
		protected ClassString defaultChildClassString;

		/* var: defaultChildClassStringSet
		 * Whether <defaultChildClassString> is set for the current element.
		 */
		protected bool defaultChildClassStringSet;

		/* var: childContextString
		 * The <ContextString> to be used by children.  Since null is a valid value, check <childContextStringSet> to see whether
		 * it's changed by this element or not.
		 */
		protected ContextString childContextString;

		/* var: childContextStringSet
		 * Whether <childContextString> is set for the current element.
		 */
		protected bool childContextStringSet;

		/* var: endingLineNumber
		 * The line number where the parent's influence ends, or -1 if it hasn't been set yet.
		 */
		protected int endingLineNumber;

		/* var: endingCharNumber
		 * The character number where the parent's influence ends, or -1 if it hasn't been set yet.  The first character is one, not 
		 * zero, and is relative to <endingLineNumber>, not the file.
		 */
		protected int endingCharNumber;

		}
	}