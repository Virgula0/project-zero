
lint:
	dotnet format --include Assets/Scripts --verify-no-changes --verbosity diagnostic 
.PHONY: lint

lint: lint