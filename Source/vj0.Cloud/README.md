# API Outline
Currently ran at port 1500. Use `localhost:1500/api/` as the base endpoint.

### `GET` /metadata/
Returns information about the currently running game.

```json
{
  "name": "{game_name}"
}
```

### `GET` /export/

RAW parameter is used to get the raw exports of an object:
```json
{
  "exports": {
    ...
  }
}
```

Without raw it can either return a texture or sound wave, or fallback to exports of the object.
