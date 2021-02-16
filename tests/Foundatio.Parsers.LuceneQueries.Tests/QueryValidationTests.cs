using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Foundatio.Parsers.ElasticQueries;
using Foundatio.Parsers.ElasticQueries.Visitors;
using Foundatio.Parsers.LuceneQueries.Nodes;
using Foundatio.Parsers.LuceneQueries.Visitors;
using Xunit;

namespace Foundatio.Parsers.LuceneQueries.Tests {
    public class QueryValidationTests {
        private static readonly HashSet<string> FieldsSet = new HashSet<string>()
        {
            // Standard Fields
            "id",
            "parentId",
            "ancestorIds",
            "libraryId",
            "typeId",
            "name",
            "description",
            "text",
            "type",
            "mediaType",
            "active",
            "publishDate",
            "expiryDate",
            "createdDate",
            "modifiedDate",
            "tags",
        };

        private static ElasticQueryVisitorContext Context = new ElasticQueryVisitorContext { QueryType = QueryType.Query, DefaultOperator = GroupOperator.Default };

        /// <summary>
        /// Check if a given query is valid.
        /// </summary>
        private async static Task<bool> CheckQueryIsValidAsync(string? query) {
            ElasticQueryParser parser = new ElasticQueryParser(conf =>
                conf.UseValidation(info => Task.FromResult(ValidateQueryInfo(info)))
            );

            try {
                IQueryNode queryNode = await parser.ParseAsync(query, Context);
                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        private static bool ValidateQueryInfo(QueryValidationInfo validationInfo) {
            if (validationInfo.IsValid == false) {
                return false;
            }

            foreach (var fieldName in validationInfo.ReferencedFields) {
                if (!string.IsNullOrWhiteSpace(fieldName) && !FieldsSet.Contains(fieldName)) {
                    return false;
                }
            }

            return true;
        }

        [InlineData("hello/world", false)]      // A Regex sequence is started and left unterminated, this should be found as invalid.
        [InlineData("hello/world/", true)]      // Valid use of the regex operator, should be found as valid.
        [InlineData("hello\\/world", true)]     // An escaped regex character which should be taken literally, this should be found as valid.
        [InlineData("hello~2world", false)]     // Proximity operator should only have numbers, and then a space before more content, this should be found as invalid.
        [InlineData("hello~2 world", true)]     // Example of valid use of the proximity operator, should be found as valid.
        [Theory]
        public async Task TestUnescapedQuotesQueryValidAsync(string query, bool validityExpectation) {
            bool isValid = await QueryValidationTests.CheckQueryIsValidAsync(query);

            if (isValid != validityExpectation) {
                throw new Exception($"isValid ({isValid}) did not match validityExpectation ({validityExpectation}) for query: {query}.");
            }
        }
    }
}