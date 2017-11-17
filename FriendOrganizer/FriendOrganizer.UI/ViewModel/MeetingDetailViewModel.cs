using FriendOrganizer.Model;
using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.View.Services;
using FriendOrganizer.UI.Wrapper;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Net.Http;
using FriendOrganizer.UI.Data.API.JokeAPI;
using FriendOrganizer.UI.Event;

namespace FriendOrganizer.UI.ViewModel
{
    public class MeetingDetailViewModel : DetailViewModelBase, IMeetingDetailViewModel
    {
        private IMeetingRepository _meetingRepository;
        private MeetingWrapper _meeting;
        private Friend _selectedAvailableFriend;
        private Friend _selectedAddedFriend;
        private List<Friend> _allFriends;
        private IJokeService _jokeService;
        private JokeWrapper _randomJoke;
        private JokeWrapper _selectedAddedJoke;
        private string _jokeNotLoadedText;

        public MeetingDetailViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IJokeService jokeService,
            IMeetingRepository meetingRepository) : base(eventAggregator, messageDialogService)
        {
            _meetingRepository = meetingRepository;
            _jokeService = jokeService;
            eventAggregator.GetEvent<AfterDetailSavedEvent>().Subscribe(AfterDetailSaved);
            eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);

            AddedFriends = new ObservableCollection<Friend>();
            AddedJokes = new ObservableCollection<JokeWrapper>();
            AvailableFriends = new ObservableCollection<Friend>();
            AddFriendCommand = new DelegateCommand(OnAddFriendExecute, OnAddFriendCanExecute);
            RemoveFriendCommand = new DelegateCommand(OnRemoveFriendExecute, OnRemoveFriendCanExecute);
            GetRandomJokeCommand = new DelegateCommand(LoadRandomJokeAsync, LoadRandomJokeCanExecute);
            AddJokeCommand = new DelegateCommand(OnAddJokeExecute, OnAddJokeCanExecute);
            OpenReadJokeCommand = new DelegateCommand(OnOpenJokeExecute, OnOpenJokeCanExecute);
            RemoveJokeCommand = new DelegateCommand(OnRemoveJokeExecute, OnRemoveJokeCanExecute);
        }

        public MeetingWrapper Meeting
        {
            get { return _meeting; }
            private set
            {
                _meeting = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddFriendCommand { get; }

        public ICommand RemoveFriendCommand { get; }

        public ICommand GetRandomJokeCommand { get; }

        public ICommand AddJokeCommand { get; }

        public ICommand OpenReadJokeCommand { get; }

        public ICommand RemoveJokeCommand { get; }

        public JokeWrapper SelectedAddedJoke
        {
            get { return _selectedAddedJoke; }
            set
            {
                _selectedAddedJoke = value;
                OnPropertyChanged();
                ((DelegateCommand) RemoveJokeCommand).RaiseCanExecuteChanged();
                ((DelegateCommand) OpenReadJokeCommand).RaiseCanExecuteChanged();
            }
        }

        private async void ShowFullJokeMessageDialogAsync()
        {
            await MessageDialogService.ShowInfoDialogAsync(
                $"{SelectedAddedJoke.setup} \n {SelectedAddedJoke.punchline}");
        }

        public ObservableCollection<Friend> AddedFriends { get; }

        public ObservableCollection<JokeWrapper> AddedJokes { get; }

        public ObservableCollection<Friend> AvailableFriends { get; }

        public Friend SelectedAvailableFriend
        {
            get { return _selectedAvailableFriend; }
            set
            {
                _selectedAvailableFriend = value;
                OnPropertyChanged();
                ((DelegateCommand) AddFriendCommand).RaiseCanExecuteChanged();
            }
        }

        public JokeWrapper RandomJoke
        {
            get { return _randomJoke; }
            set
            {
                _randomJoke = value;
                OnPropertyChanged();
                ((DelegateCommand) AddJokeCommand).RaiseCanExecuteChanged();
                ((DelegateCommand) GetRandomJokeCommand).RaiseCanExecuteChanged();
            }
        }

        public Friend SelectedAddedFriend
        {
            get { return _selectedAddedFriend; }
            set
            {
                _selectedAddedFriend = value;
                OnPropertyChanged();
                ((DelegateCommand) RemoveFriendCommand).RaiseCanExecuteChanged();
            }
        }

        public string JokeNotLoadedText
        {
            get { return _jokeNotLoadedText; }
            set
            {
                _jokeNotLoadedText = value;
                OnPropertyChanged();
            }
        }

        public override async Task LoadAsync(int meetingId)
        {
            var meeting = meetingId > 0
                ? await _meetingRepository.GetByIdAsync(meetingId)
                : CreateNewMeeting();

            Id = meetingId;

            InitializeMeeting(meeting);

            _allFriends = await _meetingRepository.GetAllFriendsAsync();

            LoadRandomJokeAsync();

            FillAddedJokesAsync();

            SetupPicklist();
        }

        private async void LoadRandomJokeAsync()
        {
            try
            {
                RandomJoke = await _jokeService.GetRandomJoke();
            }
            catch (HttpRequestException e)
            {
                await MessageDialogService.ShowInfoDialogAsync("Random jokes could not be loaded. " +
                                                               "Check your internet connection.");
                JokeNotLoadedText = "No jokes could be loaded..";
                RandomJoke = null;
            }
        }

        private async void FillAddedJokesAsync()
        {
            var jokes = await _meetingRepository.GetAllJokesForMeetingAsync(Meeting.Model.Id);

            AddedJokes.Clear();

            foreach (var joke in jokes)
            {
                AddedJokes.Add(new JokeWrapper(joke)
                {
                    type = joke.Type,
                    setup = joke.Setup,
                    punchline = joke.Punchline
                });
            }
        }

        protected async override void OnDeleteExecute()
        {
            var result =
                await MessageDialogService.ShowOkCancelDialogAsync(
                    $"Do you really want to delete the meeting {Meeting.Title}?", "Question");
            if (result == MessageDialogResult.OK)
            {
                _meetingRepository.Remove(Meeting.Model);
                await _meetingRepository.SaveAsync();
                RaiseDetailDeletedEvent(Meeting.Id);
            }
        }

        protected override bool OnSaveCanExecute()
        {
            return Meeting != null && !Meeting.HasErrors && HasChanges;
        }

        protected override async void OnSaveExecute()
        {
            await _meetingRepository.SaveAsync();
            HasChanges = _meetingRepository.HasChanges();
            Id = Meeting.Id;
            RaiseDetailSavedEvent(Meeting.Id, Meeting.Title);
        }

        private void SetupPicklist()
        {
            var meetingFriendIds = Meeting.Model.Friends.Select(f => f.Id).ToList();
            var addedFriends = _allFriends.Where(f => meetingFriendIds.Contains(f.Id)).OrderBy(f => f.FirstName);
            var availableFriends = _allFriends.Except(addedFriends).OrderBy(f => f.FirstName);

            AddedFriends.Clear();
            AvailableFriends.Clear();
            foreach (var addedFriend in addedFriends)
            {
                AddedFriends.Add(addedFriend);
            }
            foreach (var availableFriend in availableFriends)
            {
                AvailableFriends.Add(availableFriend);
            }
        }

        private bool LoadRandomJokeCanExecute()
        {
            if (RandomJoke != null)
            {
                return true;
            }
            return false;
        }

        private Meeting CreateNewMeeting()
        {
            var meeting = new Meeting
            {
                DateFrom = DateTime.Now.Date,
                DateTo = DateTime.Now.Date
            };
            _meetingRepository.Add(meeting);
            return meeting;
        }

        private async void OnAddJokeExecute()
        {
            var joke = await CreateNewJoke();

            RandomJoke = await _jokeService.GetRandomJoke();

            if (AddedJokes.Any(j => j.setup == joke.Setup && j.punchline == joke.Punchline))
            {
                await MessageDialogService.ShowInfoDialogAsync("This joke is already added to the meeting.");
                return;
            }
            Meeting.Model.Jokes.Add(joke);

            AddedJokes.Add(new JokeWrapper(joke)
            {
                type = joke.Type,
                setup = joke.Setup,
                punchline = joke.Punchline
            });

            HasChanges = true;
            ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();
        }

        private async Task<Joke> CreateNewJoke()
        {
            var joke = new Joke
            {
                Type = RandomJoke.type,
                Setup = RandomJoke.setup,
                Punchline = RandomJoke.punchline
            };

            var addedJoke = await _meetingRepository.AddJokeAsync(joke);
            return addedJoke;
        }

        private void InitializeMeeting(Meeting meeting)
        {
            Meeting = new MeetingWrapper(meeting);
            Meeting.PropertyChanged += (s, e) =>
            {
                if (!HasChanges)
                {
                    HasChanges = _meetingRepository.HasChanges();
                }

                if (e.PropertyName == nameof(Meeting.HasErrors))
                {
                    ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();
                }
                if (e.PropertyName == nameof(Meeting.Title))
                {
                    SetTitle();
                }
            };
            ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();

            if (Meeting.Id == 0)
            {
                // Little trick to trigger the validation
                Meeting.Title = "";
            }
            SetTitle();
        }

        private void SetTitle()
        {
            Title = Meeting.Title;
        }

        private void OnRemoveFriendExecute()
        {
            var friendToRemove = SelectedAddedFriend;

            Meeting.Model.Friends.Remove(friendToRemove);
            AddedFriends.Remove(friendToRemove);
            AvailableFriends.Add(friendToRemove);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnRemoveFriendCanExecute()
        {
            return SelectedAddedFriend != null;
        }

        private bool OnAddFriendCanExecute()
        {
            return SelectedAvailableFriend != null;
        }

        private void OnAddFriendExecute()
        {
            var friendToAdd = SelectedAvailableFriend;

            Meeting.Model.Friends.Add(friendToAdd);
            AddedFriends.Add(friendToAdd);
            AvailableFriends.Remove(friendToAdd);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnRemoveJokeCanExecute()
        {
            return SelectedAddedJoke != null;
        }

        private void OnRemoveJokeExecute()
        {
            var jokeToRemove = SelectedAddedJoke;

            Meeting.Model.Jokes.Remove(jokeToRemove.Model);
            AddedJokes.Remove(jokeToRemove);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnOpenJokeCanExecute()
        {
            return SelectedAddedJoke != null;
        }

        private void OnOpenJokeExecute()
        {
            if (SelectedAddedJoke != null)
            {
                ShowFullJokeMessageDialogAsync();
            }
        }

        private bool OnAddJokeCanExecute()
        {
            return RandomJoke != null;
        }

        private async void AfterDetailSaved(AfterDetailSavedEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailViewModel))
            {
                await _meetingRepository.ReloadFriendAsync(args.Id);
                _allFriends = await _meetingRepository.GetAllFriendsAsync();
                SetupPicklist();
            }
        }

        private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailViewModel))
            {
                _allFriends = await _meetingRepository.GetAllFriendsAsync();
                SetupPicklist();
            }
        }
    }
}
