// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace AutoRest.ObjectiveC.Markdown
{
    public abstract class MarkdownElement : IMarkdown
    {
        public abstract string ToMarkdown();

        public override string ToString()
        {
            return ToMarkdown();
        }
    }
}
