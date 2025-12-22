from google.protobuf.internal import containers as _containers
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class Vec3(_message.Message):
    __slots__ = ("x", "y", "z")
    X_FIELD_NUMBER: _ClassVar[int]
    Y_FIELD_NUMBER: _ClassVar[int]
    Z_FIELD_NUMBER: _ClassVar[int]
    x: float
    y: float
    z: float
    def __init__(self, x: _Optional[float] = ..., y: _Optional[float] = ..., z: _Optional[float] = ...) -> None: ...

class PlayerState(_message.Message):
    __slots__ = ("id", "position", "rotation", "timestamp")
    ID_FIELD_NUMBER: _ClassVar[int]
    POSITION_FIELD_NUMBER: _ClassVar[int]
    ROTATION_FIELD_NUMBER: _ClassVar[int]
    TIMESTAMP_FIELD_NUMBER: _ClassVar[int]
    id: str
    position: Vec3
    rotation: Vec3
    timestamp: int
    def __init__(self, id: _Optional[str] = ..., position: _Optional[_Union[Vec3, _Mapping]] = ..., rotation: _Optional[_Union[Vec3, _Mapping]] = ..., timestamp: _Optional[int] = ...) -> None: ...

class WorldState(_message.Message):
    __slots__ = ("players", "server_time")
    PLAYERS_FIELD_NUMBER: _ClassVar[int]
    SERVER_TIME_FIELD_NUMBER: _ClassVar[int]
    players: _containers.RepeatedCompositeFieldContainer[PlayerState]
    server_time: int
    def __init__(self, players: _Optional[_Iterable[_Union[PlayerState, _Mapping]]] = ..., server_time: _Optional[int] = ...) -> None: ...

class BotMoveData(_message.Message):
    __slots__ = ("id", "position")
    ID_FIELD_NUMBER: _ClassVar[int]
    POSITION_FIELD_NUMBER: _ClassVar[int]
    id: str
    position: Vec3
    def __init__(self, id: _Optional[str] = ..., position: _Optional[_Union[Vec3, _Mapping]] = ...) -> None: ...

class BotMoveBatch(_message.Message):
    __slots__ = ("bots",)
    BOTS_FIELD_NUMBER: _ClassVar[int]
    bots: _containers.RepeatedCompositeFieldContainer[BotMoveData]
    def __init__(self, bots: _Optional[_Iterable[_Union[BotMoveData, _Mapping]]] = ...) -> None: ...
