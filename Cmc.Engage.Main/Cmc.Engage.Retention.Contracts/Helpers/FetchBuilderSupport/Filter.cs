using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cmc.Engage.Contracts;
using Cmc.Engage.Models;

namespace Cmc.Engage.Retention.FetchBuilderSupport
{
	public interface IFilter
	{
		string FilterType { get; set; }
		void AddCondition(IConditionEntity condition);
		void AddCondition(string attribute, string op, object value);
		void AddCondition(string attribute, string op, object[] values);
		void AddConditions(IEnumerable<IConditionEntity> conditions);
		IFilter AddFilter();
	}

	public class Filter : IFilter
	{

		protected Filter()
		{
		}

		private XElement _filterElement;
		protected XElement FilterElement
		{
			get
			{
				if (_filterElement == null)
				{
					_filterElement = new XElement("filter");
					OnFilterElementUpdated(_filterElement);
				}
				return _filterElement;
			}

			set
			{
				if (_filterElement != value)
				{
					_filterElement = value;
					OnFilterElementUpdated(_filterElement);
				}
			}
		}

		protected virtual void OnFilterElementUpdated(XElement newValue)
		{

		}

		public string FilterType
		{
			get { return FilterElement.Attribute("type")?.Value ?? "and"; }
			set { FilterElement.SetAttributeValue("type", value); }
		}

		public void AddCondition(IConditionEntity condition)
		{
			var attributePath = condition.ParseAttributeName();
			string name = attributePath.AttributeName;

			switch ((cmc_conditiontype?)condition.cmc_conditiontype?.Value)
			{
				case cmc_conditiontype.Equals:
					if (condition.cmc_value == null)
					{
						AddCondition(name, "null");
					}
					else
					{
						AddCondition(name, "eq", condition.cmc_value);
					}

					break;

				case cmc_conditiontype.BeginsWith:
					if (!String.IsNullOrWhiteSpace(condition.cmc_value))
					{
						AddCondition(name, "like", $"{condition.cmc_value}%");
					}
					else
					{
						var filter = AddFilter();
						filter.FilterType = "or";

						var characters = charactersBetween(condition.cmc_min.First(), condition.cmc_max.First());
						foreach (var character in characters)
						{
							filter.AddCondition(name, "like", $"{character}%");
						}
					}

					break;

				case cmc_conditiontype.Range:
					AddCondition(name, "ge", condition.cmc_min);
					AddCondition(name, "le", condition.cmc_max);

					break;
			}
		}

		public void AddCondition(string attribute, string op)
		{
			var conditionElement = new XElement("condition",
				new XAttribute("attribute", attribute),
				new XAttribute("operator", op));

			FilterElement.Add(conditionElement);
		}

		public void AddCondition(string attribute, string op, object value)
		{
			if (value == null)
				return;

			var conditionElement = new XElement("condition",
				new XAttribute("attribute", attribute),
				new XAttribute("operator", op),
				new XAttribute("value", value));

			FilterElement.Add(conditionElement);
		}

		public void AddCondition(string attribute, string op, object[] values)
		{
			var conditionElement = new XElement("condition",
				new XAttribute("attribute", attribute),
				new XAttribute("operator", op),
				values.Select(v => new XElement("value", v)));

			FilterElement.Add(conditionElement);
		}

		public void AddConditions(IEnumerable<IConditionEntity> conditions)
		{
			foreach (var condition in conditions)
			{
				AddCondition(condition);
			}
		}

		public IFilter AddFilter()
		{
			var filter = new Filter();
			FilterElement.Add(filter.FilterElement);
			return filter;
		}

		private char[] charactersBetween(char start, char end)
		{
			return Enumerable.Range(start, end - start + 1).Select(c => (char)c).ToArray();
		}
	}
}
