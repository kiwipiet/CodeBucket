using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using System.Linq;
using BitbucketSharp.Models;

namespace CodeBucket.Core.ViewModels.Source
{
	public class BranchesAndTagsViewModel : LoadableViewModel
	{
		private int _selectedFilter;
		public int SelectedFilter
		{
			get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
		}

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public CollectionViewModel<ViewObject> Items { get; }

		public ICommand GoToSourceCommand
		{
			get { return new MvxCommand<ViewObject>(GoToSource); }
		}

		private void GoToSource(ViewObject obj)
		{
			var branch = obj.Object as BranchModel;
			var tag = obj.Object as TagModel;

			if (branch != null)
				ShowViewModel<SourceTreeViewModel>(new SourceTreeViewModel.NavObject { Username = Username, Repository = Repository, Branch = branch.Node });
			else if (tag != null)
				ShowViewModel<SourceTreeViewModel>(new SourceTreeViewModel.NavObject { Username = Username, Repository = Repository, Branch = tag.Node });
		}

		public BranchesAndTagsViewModel()
		{
            Items = new CollectionViewModel<ViewObject>();
            this.Bind(x => x.SelectedFilter).Subscribe(x => LoadCommand.Execute(false));
		}

		public void Init(NavObject navObject)
		{
			Username = navObject.Username;
			Repository = navObject.Repository;
			_selectedFilter = navObject.IsShowingBranches ? 0 : 1;
		}

		protected override Task Load(bool forceCacheInvalidation)
		{
			if (SelectedFilter == 0)
			{
				return this.RequestModel(() => this.GetApplication().Client.Users[Username].Repositories[Repository].Branches.GetBranches(forceCacheInvalidation), response =>
				{
						//this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
						Items.Items.Reset(response.Values.OrderBy(x => x.Branch).Select(x => new ViewObject { Name = x.Branch, Object = x }));
				});
			}
			else
			{
				return this.RequestModel(() => this.GetApplication().Client.Users[Username].Repositories[Repository].GetTags(forceCacheInvalidation), response => 
				{
						//this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
						Items.Items.Reset(response.Select(x => new ViewObject { Name = x.Key, Object = x.Value }));
				});
			}
		}

		public class ViewObject
		{
			public string Name { get; set; }
			public object Object { get; set; }
		}

		public class NavObject
		{
			public string Username { get; set; }
			public string Repository { get; set; }
			public bool IsShowingBranches { get; set; }

			public NavObject()
			{
				IsShowingBranches = true;
			}
		}
	}
}

