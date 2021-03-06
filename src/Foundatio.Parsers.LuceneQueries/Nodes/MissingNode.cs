﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Foundatio.Parsers.LuceneQueries.Nodes {
    public class MissingNode : QueryNodeBase, IFieldQueryNode {
        public bool? IsNegated { get; set; }
        public string Prefix { get; set; }
        public string Field { get; set; }

        public MissingNode CopyTo(MissingNode target) {
            if (IsNegated.HasValue)
                target.IsNegated = IsNegated;

            if (Prefix != null)
                target.Prefix = Prefix;

            if (Field != null)
                target.Field = Field;

            foreach (var kvp in Data)
                target.Data.Add(kvp.Key, kvp.Value);

            return target;
        }

        public override string ToString() {
            var builder = new StringBuilder();

            if (IsNegated.HasValue && IsNegated.Value)
                builder.Append("NOT ");

            builder.Append(Prefix);
            builder.Append("_missing_");
            builder.Append(":");
            builder.Append(this.Field);

            return builder.ToString();
        }

        public override IEnumerable<IQueryNode> Children => EmptyNodeList;
    }
}