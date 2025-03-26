
lint:
	dotnet format --include Assests/Scripts --verify-no-changes --verbosity diagnostic 
.PHONY: lint

lint: lint